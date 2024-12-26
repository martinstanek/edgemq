using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using EdgeMq.Model;

namespace EdgeMq.Client;

public sealed class EdgeMqClient : IEdgeMqClient
{
    private readonly Lazy<IEdgeMqApi> _api;

    public EdgeMqClient(HttpClient httpClient)
    {
        _api = new Lazy<IEdgeMqApi>(() => Refit.RestService.For<IEdgeMqApi>(httpClient));
    }

    public Task<QueueMetrics> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        return _api.Value.GetMetricsAsync(queueName);
    }

    public Task<QueueMetrics> EnqueueAsync(string queueName, string payload)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NullOrWhiteSpace(payload);

        return _api.Value.EnqueueAsync(queueName, payload);
    }

    public Task<QueueMetrics> AcknowledgeAsync(string queueName, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        return _api.Value.AcknowledgeAsync(queueName, batchId);
    }

    public Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        return _api.Value.DequeueAsync(queueName, batchSize);
    }

    public Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        return _api.Value.PeekAsync(queueName, batchSize);
    }
}