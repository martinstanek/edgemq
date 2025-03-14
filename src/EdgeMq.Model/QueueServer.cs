using System.Collections.Immutable;

namespace EdgeMq.Model;

public sealed record QueueServer
{
    public required string Version { get; init; } = string.Empty;

    public required string ConstraintsViolationMode { get; init; } = string.Empty;

    public required ulong UptimeSeconds { get; init; }

    public required ImmutableArray<Queue> Queues { get; init; } = [];

    public static QueueServer Empty => new()
    {
        Version = string.Empty,
        UptimeSeconds = 0,
        ConstraintsViolationMode = string.Empty,
        Queues = []
    };
}