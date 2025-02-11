using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    private readonly EdgeMqServerConfiguration _configuration;
    private readonly QueueManager _queueManager;
    private readonly DateTime _started = DateTime.Now;

    public EdgeQueueHandler(QueueManager queueManager, EdgeMqServerConfiguration configuration)
    {
        _queueManager = queueManager;
        _configuration = configuration;
    }

    public async Task<IResult> AcknowledgeAsync(string queueName, string apiKey, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        if (IsNotAuthorized(apiKey, out var authResult))
        {
            return authResult;
        }

        await _queueManager[queueName].AcknowledgeAsync(batchId, CancellationToken.None);

        return Results.NoContent();
    }

    public async Task<IResult> EnqueueAsync(HttpRequest request, string queueName, string apiKey)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        if (IsNotAuthorized(apiKey, out var authResult))
        {
            return authResult;
        }

        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var rawContent = await reader.ReadToEndAsync();
        var headers = request.Headers
            .ToDictionary(k => k.Key, v => v.Value.FirstOrDefault() ?? string.Empty)
            .Where(p => p.Key.StartsWith(EdgeHeaderPrefix))
            .ToDictionary(k => k.Key.Replace(EdgeHeaderPrefix, string.Empty), v => v.Value);

        var added = await _queueManager[queueName].EnqueueAsync(rawContent, headers, CancellationToken.None);

        return !added && _configuration.ConstraintsMode == QueueApiConstraintsMode.Fail
            ? Results.UnprocessableEntity()
            : Results.Ok(new QueueEnqueueResult(added));
    }

    public async Task<IResult> DequeueAsync(string queueName, string apiKey, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        if (IsNotAuthorized(apiKey, out var authResult))
        {
            return authResult;
        }

        var messages = await _queueManager[queueName].DequeueAsync(batchSize: (uint) batchSize, CancellationToken.None);
        var result = messages.Select(s => new QueueRawMessage
        {
            Id = s.Id,
            BatchId = s.BatchId,
            Payload = s.Payload,
            Headers = s.Headers
        }).ToImmutableArray();

        return Results.Ok(result);
    }

    public async Task<IResult> PeekAsync(string queueName, string apiKey, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        if (IsNotAuthorized(apiKey, out var authResult))
        {
            return authResult;
        }

        var messages = await _queueManager[queueName].PeekAsync(batchSize: (uint) batchSize, CancellationToken.None);
        var result = messages.Select(s => new QueueRawMessage
        {
            Id = s.Id,
            BatchId = s.BatchId,
            Payload = s.Payload,
            Headers = s.Headers
        }).ToImmutableArray();

        return Results.Ok(result);
    }

    public Task<IResult> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var result = GetQueueMetrics(queueName);

        return Task.FromResult(Results.Ok(result));
    }

    public Task<IResult> GetQueuesAsync()
    {
        var queues = new List<Queue>();

        foreach (var queue in _queueManager.Queues)
        {
            queues.Add(new Queue
            {
                Name = queue.Name,
                StoreMode = queue.Configuration.IsInMemory ? "InMemory" : "FileSystem",
                Metrics = GetQueueMetrics(queue.Name)
            });
        }

        var result = new QueueServer
        {
            Queues = queues.ToImmutableArray(),
            UptimeSeconds = (ulong) Math.Round(DateTime.Now.Subtract(_started).TotalSeconds, 0),
            ConstraintsViolationMode = _configuration.ConstraintsMode.ToString(),
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty
        };

        return Task.FromResult(Results.Ok(result));
    }

    private QueueMetrics GetQueueMetrics(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var queue = _queueManager[queueName];

        return new QueueMetrics
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
    }

    private bool IsNotAuthorized(string apiKey, out IResult notAuthorizedResult)
    {
        notAuthorizedResult = Results.NoContent();

        if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
        {
            return false;
        }

        var notAuthorized = !_configuration.ApiKey.Equals(apiKey, StringComparison.Ordinal);

        if (notAuthorized)
        {
            notAuthorizedResult = Results.Unauthorized();
        }

        return notAuthorized;
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