using System.Runtime.InteropServices;
using Ardalis.GuardClauses;
using EdgeMq.Client;
using EdgeMq.TestContainer.Docker;

namespace EdgeMq.TestContainer;

public sealed class EdgeQueueTestContainer : IAsyncDisposable
{
    private const string EdgeQueueImageFullName = "awitec/edgemq:latest-arm64";
    private const string EdgeQueueTestQueueName = "testcontainer-queue";
    private const string EdgeQueueContainerName = "edgemq-test";
    private const string EdgeQueueUrl = "http://localhost:2323";
    private const int EdgeQueueContainerStartUpDelaySeconds = 5;

    private readonly IDockerService _dockerService = new DockerService();
    private IEdgeMqClient? _client;
    private bool _isDisposed;

    public async Task<IEdgeMqClient> GetClientAsync(
        string testContainerName =  EdgeQueueContainerName,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(testContainerName);

        if (_client is not null)
        {
            return _client;
        }

        var running = await _dockerService.IsDockerRunningAsync(cancellationToken);

        if (!running)
        {
            throw new InvalidOperationException("The Docker daemon seems not to be running");
        }

        var hostNetwork = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        await _dockerService.PullImageAsync(EdgeQueueImageFullName, cancellationToken);
        await _dockerService.StartContainerAsync(
            fullImageName: EdgeQueueImageFullName,
            containerName: testContainerName,
            hostNetwork: hostNetwork,
            ports: new Dictionary<ushort, ushort> { { 2323, 2323 } },
            volumes: new Dictionary<string, string> { { "edgemqdata", "/data" } },
            variables: new Dictionary<string, string> { { "EDGEMQ_QUEUES", EdgeQueueTestQueueName }, { "EDGEMQ_MODE", "InMemory" } },
            cancellationToken: cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(EdgeQueueContainerStartUpDelaySeconds), cancellationToken);

        var containerUrl = new Uri(EdgeQueueUrl);
        var httpClient = new HttpClient { BaseAddress = containerUrl };

        _client = new EdgeMqClient(httpClient);

        return _client;
    }

    public Task<bool> IsTestable()
    {
        return _dockerService.IsDockerRunningAsync(CancellationToken.None);
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