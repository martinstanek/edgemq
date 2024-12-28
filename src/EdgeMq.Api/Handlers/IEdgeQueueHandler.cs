using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdgeMq.Model;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public interface IEdgeQueueHandler
{
    Task<QueueMetrics> GetMetricsAsync(string queueName);

    Task<QueueMetrics> EnqueueAsync(HttpRequest request, string queueName);

    Task<QueueMetrics> AcknowledgeAsync(string queueName, Guid batchId);

    Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize);

    Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize);

    Task<IReadOnlyCollection<Queue>> GetQueuesAsync();
}