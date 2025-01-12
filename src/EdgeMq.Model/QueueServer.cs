using System.Collections.Generic;

namespace EdgeMq.Model;

public sealed record QueueServer
{
    public required string Version { get; init; } = string.Empty;

    public required string ConstraintsViolationMode { get; init; } = string.Empty;

    public required IReadOnlyCollection<Queue> Queues { get; init; } = [];

    public static QueueServer Empty => new QueueServer
    {
        Version = string.Empty,
        ConstraintsViolationMode = string.Empty,
        Queues = []
    };
}