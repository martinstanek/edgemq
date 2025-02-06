using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using EdgeMq.Model;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public interface IEdgeQueueHandler
{
    Task AcknowledgeAsync(string queueName, Guid batchId);

    Task<bool> EnqueueAsync(HttpRequest request, string queueName);

    Task<QueueMetrics> GetMetricsAsync(string queueName);

    Task<ImmutableArray<QueueRawMessage>> DequeueAsync(string queueName, int batchSize);

    Task<ImmutableArray<QueueRawMessage>> PeekAsync(string queueName, int batchSize);

    Task<QueueServer> GetQueuesAsync();
}