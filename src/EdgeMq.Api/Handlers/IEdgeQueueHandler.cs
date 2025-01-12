using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdgeMq.Model;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public interface IEdgeQueueHandler
{
    Task AcknowledgeAsync(string queueName, Guid batchId);

    Task<bool> EnqueueAsync(HttpRequest request, string queueName);

    Task<QueueMetrics> GetMetricsAsync(string queueName);

    Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize);

    Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize);

    Task<IReadOnlyCollection<Queue>> GetQueuesAsync();
}