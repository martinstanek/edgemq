using System.Text;
using System.Diagnostics;
using Ardalis.GuardClauses;

namespace EdgeMq.TestContainer.Docker;

public sealed class DockerService : IDockerService
{
    public Task StartContainerAsync(
        string fullImageName,
        string containerName,
        IReadOnlyDictionary<ushort, ushort> ports,
        IReadOnlyDictionary<string, string> volumes,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(containerName);

        var commandArguments = new StringBuilder($"run -d --name {containerName} --network=host");

        commandArguments.Append(string.Join(' ', ports.Select(p => $" -p {p.Key}:{p.Value}")));
        commandArguments.Append(string.Join(' ', volumes.Select(v => $" -v \"{v.Key}:{v.Value}\"")));
        commandArguments.Append(string.Join(' ', variables.Select(e => $" -e \"{e.Key}={e.Value}\"")));
        commandArguments.Append($" {fullImageName}");

        var arguments = commandArguments.ToString();

        return RunDockerProcessAsync(arguments, cancellationToken);
    }

    public async Task<bool> IsDockerRunningAsync(CancellationToken cancellationToken)
    {
        var limitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitTokenSource.Token);

        try
        {
            var stats = await GetDockerProcessesAsync(mergedTokenSource.Token);

            return !string.IsNullOrWhiteSpace(stats);
        }
        catch
        {
            return false;
        }
    }

    public Task<string> GetVersionAsync(CancellationToken cancellationToken)
    {
        return RunDockerProcessAsync("--version", cancellationToken);
    }

    public Task<string> GetDockerProcessesAsync(CancellationToken cancellationToken)
    {
        return RunDockerProcessAsync("ps", cancellationToken);
    }

    public Task<string> PullImageAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        return RunDockerProcessAsync($"pull {fullImageName}", cancellationToken);
    }

    public async Task RemoveContainerAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        var id = await RunDockerProcessAsync($"ps -a -q --filter ancestor=\"{fullImageName}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        await RunDockerProcessAsync($"container rm {id}", cancellationToken);
    }

    public async Task StopContainerAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        var id = await RunDockerProcessAsync($"ps -a -q --filter ancestor=\"{fullImageName}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        await RunDockerProcessAsync($"container stop {id}", cancellationToken);
    }

    public Task RemoveImageAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        return RunDockerProcessAsync($"image rm {fullImageName}", cancellationToken);
    }

    private static Task<string> RunDockerProcessAsync(string arguments, CancellationToken cancellationToken)
    {
        return RunProcessAsync("docker", arguments, cancellationToken);
    }

    private static async Task<string> RunProcessAsync(string name, string arguments, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = name,
            Arguments = arguments,
            RedirectStandardOutput = true
        };

        var process = Process.Start(info);

        if (process is null)
        {
            throw new InvalidOperationException("Process can not be executed");
        }

        var output = string.Empty;

        while (!process.StandardOutput.EndOfStream)
        {
            output = await process.StandardOutput.ReadLineAsync(cancellationToken);
        }

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Process did not exited with the expected response code");
        }

        return output ?? string.Empty;
    }
}