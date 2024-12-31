using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using EdgeMq.Model;
using EdgeMq.Service;

namespace EdgeMq.Api.Handlers;

public sealed class EdgeQueueHandler : IEdgeQueueHandler
{
    private readonly QueueManager _queueManager;

    public EdgeQueueHandler(QueueManager queueManager)
    {
        _queueManager = queueManager;
    }

    public Task<QueueMetrics> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var queue = _queueManager.GetQueue(queueName);
        var result = new QueueMetrics
        {
            Name = queue.Name,
            MessageCount = queue.MessageCount,
            MaxMessageCount = queue.MaxMessageCount,
            MessagesSizeBytes = queue.MessageSizeBytes,
            MaxMessagesSizeBytes = queue.MaxMessageSizeBytes,
            BufferMessageCount = queue.BufferMessageCount,
            MaxBufferMessageCount = queue.MaxBufferMessageCount,
            BufferMessagesSizeBytes = queue.BufferMessageSizeBytes,
            MaxBufferMessagesSizeBytes = queue.MaxBufferMessageSizeBytes,
            MessageCountPressure = SafeDivide(queue.MaxMessageCount, queue.MessageCount),
            MessagesSizePressure = SafeDivide(queue.MaxMessageSizeBytes, queue.MessageSizeBytes),
            BufferMessageCountPressure = SafeDivide(queue.MaxBufferMessageCount, queue.BufferMessageCount),
            BufferMessagesSizePressure = SafeDivide(queue.MaxBufferMessageSizeBytes, queue.BufferMessageSizeBytes),
            MessagesInPerSecond = queue.MessagesInPerSecond,
            MessagesOutPerSecond = queue.MessagesOutPerSecond
        };

        return Task.FromResult(result);
    }

    public async Task<QueueMetrics> EnqueueAsync(HttpRequest request, string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var rawContent = await reader.ReadToEndAsync();
        var queue = _queueManager.GetQueue(queueName);

        await queue.QueueAsync(rawContent, CancellationToken.None);

        return await GetMetricsAsync(queueName);
    }

    public async Task<QueueMetrics> AcknowledgeAsync(string queueName, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var queue = _queueManager.GetQueue(queueName);

        await queue.AcknowledgeAsync(batchId, CancellationToken.None);

        return await GetMetricsAsync(queueName);
    }

    public async Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var queue = _queueManager.GetQueue(queueName);
        var messages = await queue.DeQueueAsync(batchSize: (uint) batchSize, CancellationToken.None);
        var result = messages.Select(s => new QueueRawMessage
        {
            Id = s.Id,
            BatchId = s.BatchId,
            Payload = s.Payload
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
            Payload = s.Payload
        });

        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Queue>> GetQueuesAsync()
    {
        var result = new List<Queue>();

        foreach (var queue in _queueManager.Queues)
        {
            result.Add(new Queue
            {
                Name = queue,
                Mode = _queueManager.IsInMemory ? "InMemory" : "FileSystem",
                Metrics = await GetMetricsAsync(queue)
            });
        }

        return result;
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