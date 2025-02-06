using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Model;

namespace EdgeMq.Client;

public interface IEdgeMqClient
{
    Task<QueueServer> GetQueuesAsync();

    Task<QueueMetrics> GetMetricsAsync(string queueName);

    Task<QueueEnqueueResult> EnqueueAsync(string queueName, string payload);

    Task<QueueEnqueueResult> EnqueueAsync(string queueName, string payload, IReadOnlyDictionary<string, string> headers);

    Task AcknowledgeAsync(string queueName, Guid batchId);

    Task<ImmutableArray<QueueRawMessage>> PeekAsync(string queueName, int batchSize);

    Task<ImmutableArray<QueueRawMessage>> DequeueAsync(string queueName, int batchSize);

    Task DequeueAsync(
        string queueName,
        int batchSize,
        TimeSpan timeOut,
        Func<ImmutableArray<QueueRawMessage>, Task> process,
        CancellationToken cancellationToken);
}