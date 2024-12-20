namespace EdgeMQ.Service;

public sealed record EdgeQueueConfiguration
{
    public required string Name { get; init; }
}