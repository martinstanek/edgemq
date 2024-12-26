using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using EdgeMq.Model;
using EdgeMQ.Service;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public sealed class EdgeQueueHandler
{
    private readonly IEdgeMq _edgeMq;

    public EdgeQueueHandler(IEdgeMq edgeMq)
    {
        _edgeMq = edgeMq;
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
        var bytes = Convert.FromBase64String(rawContent);

        await _edgeMq.QueueAsync(bytes, CancellationToken.None);

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
            PayloadBase64 = Convert.ToBase64String(s.Payload)
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
            PayloadBase64 = Convert.ToBase64String(s.Payload)
        });

        return result.ToArray();
    }
}