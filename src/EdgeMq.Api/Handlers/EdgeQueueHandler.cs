using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using EdgeMq.Api.Configuration;
using EdgeMq.Model;
using EdgeMq.Service;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public sealed class EdgeQueueHandler : IEdgeQueueHandler
{
    private readonly EdgeMqServerConfiguration _serverConfiguration;
    private readonly IEdgeMq _edgeMq;

    public EdgeQueueHandler(IEdgeMq edgeMq, EdgeMqServerConfiguration serverConfiguration)
    {
        _edgeMq = edgeMq;
        _serverConfiguration = serverConfiguration;
    }

    public Task<QueueMetrics> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var result = new QueueMetrics
        {
            Name = _edgeMq.Name,
            MessageCount = _edgeMq.MessageCount
        };

        return Task.FromResult(result);
    }

    public async Task<QueueMetrics> EnqueueAsync(HttpRequest request, string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var rawContent = await reader.ReadToEndAsync();

        await _edgeMq.QueueAsync(rawContent, CancellationToken.None);

        return new QueueMetrics
        {
            Name = _edgeMq.Name,
            MessageCount = _edgeMq.MessageCount
        };
    }

    public async Task<QueueMetrics> AcknowledgeAsync(string queueName, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        await _edgeMq.AcknowledgeAsync(batchId, CancellationToken.None);

        return new QueueMetrics
        {
            Name = _edgeMq.Name,
            MessageCount = _edgeMq.MessageCount
        };
    }

    public async Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var messages = await _edgeMq.DeQueueAsync(batchSize: (uint) batchSize, CancellationToken.None);

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

        var messages = await _edgeMq.PeekAsync(batchSize: (uint) batchSize, CancellationToken.None);

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

        foreach (var queue in _serverConfiguration.Queues)
        {
            result.Add(new Queue
            {
                Name = queue,
                Mode = _serverConfiguration.Mode.ToString(),
                Metrics = await GetMetricsAsync(queue)
            });
        }

        return result;
    }
}