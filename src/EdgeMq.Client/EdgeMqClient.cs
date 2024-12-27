using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using EdgeMq.Model;

namespace EdgeMq.Client;

public sealed class EdgeMqClient : IEdgeMqClient
{
    private readonly Lazy<IEdgeMqApi> _api;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

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

    public Task<IReadOnlyCollection<QueueRawMessage>> PeekAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        return _api.Value.PeekAsync(queueName, batchSize);
    }

    public Task<IReadOnlyCollection<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        return _api.Value.DequeueAsync(queueName, batchSize);
    }

    public async Task DequeueAsync(
        string queueName,
        int batchSize,
        TimeSpan timeOut,
        Func<IReadOnlyCollection<QueueRawMessage>, Task> process,
        CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);
        Guard.Against.NegativeOrZero(timeOut.Seconds);

        await _semaphore.WaitAsync(cancellationToken);

        var timeOutSource = new CancellationTokenSource(timeOut);
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeOutSource.Token, cancellationToken);
        var linkedToken = linkedSource.Token;

        try
        {
            var messages = await PeekAsync(queueName, batchSize);

            await Task.Run(() => process(messages), linkedToken);

            if (messages.Count != 0)
            {
                return;
            }

            var batchId = messages.First().BatchId;

            await AcknowledgeAsync(queueName, batchId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}