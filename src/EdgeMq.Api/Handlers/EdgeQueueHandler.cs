using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using EdgeMq.Api.Configuration;
using EdgeMq.Model;
using EdgeMq.Service;

namespace EdgeMq.Api.Handlers;

public sealed class EdgeQueueHandler : IEdgeQueueHandler
{
    private const string EdgeHeaderPrefix = "EDGQ_";

    private readonly QueueManager _queueManager;
    private readonly EdgeMqServerConfiguration _configuration;
    private readonly DateTime _started = DateTime.Now;

    public EdgeQueueHandler(QueueManager queueManager, EdgeMqServerConfiguration configuration)
    {
        _queueManager = queueManager;
        _configuration = configuration;
    }

    public async Task AcknowledgeAsync(string queueName, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var queue = _queueManager.GetQueue(queueName);

        await queue.AcknowledgeAsync(batchId, CancellationToken.None);
    }

    public async Task<bool> EnqueueAsync(HttpRequest request, string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var rawContent = await reader.ReadToEndAsync();
        var headers = request.Headers
            .ToDictionary(k => k.Key, v => v.Value.FirstOrDefault() ?? string.Empty)
            .Where(p => p.Key.StartsWith(EdgeHeaderPrefix))
            .ToDictionary(k => k.Key.Replace(EdgeHeaderPrefix, string.Empty), v => v.Value);

        var queue = _queueManager.GetQueue(queueName);

        return await queue.EnqueueAsync(rawContent, headers, CancellationToken.None);
    }

    public Task<QueueMetrics> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var queue = _queueManager.GetQueue(queueName);
        var result = new QueueMetrics
        {
            Name = queue.Name,
            MessageCount = queue.Metrics.MessageCount,
            MaxMessageCount = queue.Metrics.MaxMessageCount,
            MessagesSizeBytes = queue.Metrics.MessageSizeBytes,
            ProcessedMessages = queue.Metrics.ProcessedMessages,
            MaxMessagesSizeBytes = queue.Metrics.MaxMessageSizeBytes,
            BufferMessageCount = queue.Metrics.BufferMessageCount,
            MaxBufferMessageCount = queue.Metrics.MaxBufferMessageCount,
            BufferMessagesSizeBytes = queue.Metrics.BufferMessageSizeBytes,
            MaxBufferMessagesSizeBytes = queue.Metrics.MaxBufferMessageSizeBytes,
            MessageCountPressure = SafeDivide(queue.Metrics.MessageCount, queue.Metrics.MaxMessageCount),
            MessagesSizePressure = SafeDivide(queue.Metrics.MessageSizeBytes, queue.Metrics.MaxMessageSizeBytes),
            BufferMessageCountPressure = SafeDivide(queue.Metrics.BufferMessageCount, queue.Metrics.MaxBufferMessageCount),
            BufferMessagesSizePressure = SafeDivide(queue.Metrics.BufferMessageSizeBytes, queue.Metrics.MaxBufferMessageSizeBytes),
            MessagesInPerSecond = queue.Metrics.MessagesInPerSecond,
            MessagesOutPerSecond = queue.Metrics.MessagesOutPerSecond,
        };

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var queue = _queueManager.GetQueue(queueName);
        var messages = await queue.DequeueAsync(batchSize: (uint) batchSize, CancellationToken.None);
        var result = messages.Select(s => new QueueRawMessage
        {
            Id = s.Id,
            BatchId = s.BatchId,
            Payload = s.Payload,
            Headers = s.Headers
        });

        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var queue = _queueManager.GetQueue(queueName);
        var messages = await queue.PeekAsync(batchSize: (uint) batchSize, CancellationToken.None);
        var result = messages.Select(s => new QueueRawMessage
        {
            Id = s.Id,
            BatchId = s.BatchId,
            Payload = s.Payload,
            Headers = s.Headers
        });

        return result.ToArray();
    }

    public async Task<QueueServer> GetQueuesAsync()
    {
        var queues = new List<Queue>();

        foreach (var queue in _queueManager.Queues)
        {
            queues.Add(new Queue
            {
                Name = queue,
                StoreMode = _queueManager.IsInMemory ? "InMemory" : "FileSystem",
                Metrics = await GetMetricsAsync(queue)
            });
        }

        return new QueueServer
        {
            Queues = queues,
            UptimeSeconds = (ulong) Math.Round(DateTime.Now.Subtract(_started).TotalSeconds, 0),
            ConstraintsViolationMode = _configuration.ConstraintsMode.ToString(),
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty
        };
    }

    private static double SafeDivide(ulong max, ulong value)
    {
        if (max == 0 || value == 0)
        {
            return 0d;
        }

        return max / (double) value;
    }
}