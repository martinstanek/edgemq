namespace EdgeMq.TestContainer.Docker;

public interface IDockerService
{
    Task StartContainerAsync(
        string fullImageName,
        string containerName,
        IReadOnlyDictionary<ushort, ushort> ports,
        IReadOnlyDictionary<string, string> volumes,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken);

    Task<bool> IsDockerRunningAsync(CancellationToken cancellationToken);

    Task<string> GetVersionAsync(CancellationToken cancellationToken);

    Task<string> GetDockerProcessesAsync(CancellationToken cancellationToken);

    Task<string> PullImageAsync(string fullImageName, CancellationToken cancellationToken);

    Task RemoveContainerAsync(string fullImageName, CancellationToken cancellationToken);

    Task StopContainerAsync(string fullImageName, CancellationToken cancellationToken);

    Task RemoveImageAsync(string fullImageName, CancellationToken cancellationToken);
}