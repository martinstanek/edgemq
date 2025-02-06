using System;
using System.Linq;
using System.Text;
using System.Net.Mime;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using EdgeMq.Model;
using EdgeMq.Client.Exceptions;
using Ardalis.GuardClauses;

namespace EdgeMq.Client;

public sealed class EdgeMqClient : IEdgeMqClient
{
    private const string EdgeHeaderPrefix = "EDGQ_";

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HttpClient _httpClient;

    public EdgeMqClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueueServer> GetQueuesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<QueueServer>("/queue");

        return result ?? throw new EdgeClientException("Invalid response content");
    }

    public async Task<QueueMetrics> GetMetricsAsync(string queueName)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        var result = await _httpClient.GetFromJsonAsync<QueueMetrics>($"/queue/{queueName}/stats");

        return result ?? throw new EdgeClientException("Invalid response content");
    }

    public Task<QueueEnqueueResult> EnqueueAsync(string queueName, string payload)
    {
        return EnqueueAsync(queueName, payload, ReadOnlyDictionary<string, string>.Empty);
    }

    public async Task<QueueEnqueueResult> EnqueueAsync(string queueName, string payload, IReadOnlyDictionary<string, string> headers)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NullOrWhiteSpace(payload);

        var oldHeaders = _httpClient.DefaultRequestHeaders.Where(s => s.Key.StartsWith(EdgeHeaderPrefix));

        foreach (var oldHeader in oldHeaders)
        {
            _httpClient.DefaultRequestHeaders.Remove(oldHeader.Key);
        }

        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.Add($"{EdgeHeaderPrefix}{header.Key}" , [header.Value]);
        }

        using var content = new StringContent(payload, Encoding.UTF8, MediaTypeNames.Text.Plain);
        using var response = await _httpClient.PostAsync($"/queue/{queueName}", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new EdgeClientException("Invalid response status", response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<QueueEnqueueResult>();

        return result ?? throw new EdgeClientException("Invalid response content");
    }

    public Task AcknowledgeAsync(string queueName, Guid batchId)
    {
        Guard.Against.NullOrWhiteSpace(queueName);

        return _httpClient.PatchAsync($"/queue/{queueName}?batchId={batchId.ToString()}", content: null);
    }

    public async Task<ImmutableArray<QueueRawMessage>> PeekAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var result = await _httpClient
            .GetFromJsonAsync<IEnumerable<QueueRawMessage>>($"/queue/{queueName}/peek?batchSize={batchSize}");

        return result?.ToImmutableArray() ?? throw new EdgeClientException("Invalid response");
    }

    public async Task<ImmutableArray<QueueRawMessage>> DequeueAsync(string queueName, int batchSize)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);

        var result = await _httpClient
            .GetFromJsonAsync<IEnumerable<QueueRawMessage>>($"/queue/{queueName}?batchSize={batchSize}");

        return result?.ToImmutableArray() ?? throw new EdgeClientException("Invalid response");
    }

    public async Task DequeueAsync(
        string queueName,
        int batchSize,
        TimeSpan timeOut,
        Func<ImmutableArray<QueueRawMessage>, Task> process,
        CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(queueName);
        Guard.Against.NegativeOrZero(batchSize);
        Guard.Against.NegativeOrZero(timeOut.TotalSeconds);

        var timeOutSource = new CancellationTokenSource(timeOut);
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeOutSource.Token, cancellationToken);
        var linkedToken = linkedSource.Token;

        await _semaphore.WaitAsync(linkedToken);

        try
        {
            var messages = await PeekAsync(queueName, batchSize);

            await Task.Run(() => process(messages), linkedToken);

            if (messages.Length == 0)
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