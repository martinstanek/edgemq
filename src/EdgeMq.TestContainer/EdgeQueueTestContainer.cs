using Ardalis.GuardClauses;
using EdgeMq.Client;
using EdgeMq.TestContainer.Docker;

namespace EdgeMq.TestContainer;

public sealed class EdgeQueueTestContainer : IAsyncDisposable
{
    private const string EdgeQueueImageFullName = "awitec/edgemq:latest-arm64";
    private const string EdgeQueueTestQueueName = "testcontainer-queue";
    private const string EdgeQueueUrl = "http://localhost:2323";

    private readonly IDockerService _dockerService = new DockerService();
    private IEdgeMqClient? _client;
    private bool _isDisposed;

    public async Task<IEdgeMqClient> GetClientAsync(string testContainerName = "edgemq-test", bool hostNetwork = true)
    {
        Guard.Against.NullOrWhiteSpace(testContainerName);

        if (_client is not null)
        {
            return _client;
        }

        var running = await _dockerService.IsDockerRunningAsync(CancellationToken.None);

        if (!running)
        {
            throw new InvalidOperationException("The Docker daemon seems not to be running");
        }

        await _dockerService.PullImageAsync(EdgeQueueImageFullName, CancellationToken.None);
        await _dockerService.StartContainerAsync(
            fullImageName: EdgeQueueImageFullName,
            containerName: testContainerName,
            hostNetwork: hostNetwork,
            ports: new Dictionary<ushort, ushort> { { 2323, 2323 } },
            volumes: new Dictionary<string, string> { { "edgemqdata", "/data" } },
            variables: new Dictionary<string, string> { { "EDGEMQ_QUEUES", EdgeQueueTestQueueName }, { "EDGEMQ_MODE", "InMemory" } },
            cancellationToken: CancellationToken.None);

        var containerUrl = new Uri(EdgeQueueUrl);
        var httpClient = new HttpClient { BaseAddress = containerUrl };

        _client = new EdgeMqClient(httpClient);

        return _client;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed || _client is null)
        {
            return;
        }

        _isDisposed = true;

        await _dockerService.StopContainerAsync(EdgeQueueImageFullName, CancellationToken.None);
        await _dockerService.RemoveContainerAsync(EdgeQueueImageFullName, CancellationToken.None);
        await _dockerService.RemoveImageAsync(EdgeQueueImageFullName, CancellationToken.None);
    }
}