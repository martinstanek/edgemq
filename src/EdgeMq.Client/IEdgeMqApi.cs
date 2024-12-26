using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdgeMq.Model;
using Refit;

namespace EdgeMq.Client;

public interface IEdgeMqApi
{
    [Get("/queue/{name}")]
    Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string name, [Query] int batchSize);

    [Get("/queue/{name}/peek")]
    Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string name, [Query] int batchSize);

    [Get("/queue/{name}/stats")]
    Task<QueueMetrics> GetMetricsAsync(string name);

    [Put("/queue/{name}")]
    Task<QueueMetrics> EnqueueAsync(string name, [Body] string payload);

    [Patch("/queue/{name}")]
    Task<QueueMetrics> AcknowledgeAsync(string name, [Query] Guid batchId);
}