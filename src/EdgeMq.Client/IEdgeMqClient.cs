using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Model;

namespace EdgeMq.Client;

public interface IEdgeMqClient
{
    Task<IReadOnlyCollection<Queue>> GetQueuesAsync();

    Task<QueueMetrics> GetMetricsAsync(string queueName);

    Task<QueueMetrics> EnqueueAsync(string queueName, string payload);

    Task<QueueMetrics> AcknowledgeAsync(string queueName, Guid batchId);

    Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize);

    Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize);

    Task<QueueMetrics> DequeueAsync(
        string queueName,
        int batchSize,
        TimeSpan timeOut,
        Func<IReadOnlyCollection<QueueRawMessage>, Task> process,
        CancellationToken cancellationToken);
}