namespace EdgeMq.Service1;

public sealed record EdgeQueueConfiguration
{
    public required string Name { get; init; }
}