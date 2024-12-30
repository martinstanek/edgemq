namespace EdgeMq.Model;

public sealed record QueueMetrics
{
    public required string Name { get; init; } = string.Empty;
    
    public required ulong MessageCount { get; init; }

    public required ulong MaxMessageCount { get; init; }

    public required ulong StoreSizeBytes { get; init; }

    public required ulong MaxStoreBytes { get; init; }

    public required ulong InputBufferMessageCount { get; init; }

    public required double MessagesInPerSecond { get; init; }

    public required double MessagesOutPerSecond { get; init; }

    public static QueueMetrics Empty => new()
    {
        Name = string.Empty,
        MessageCount = 0,
        MaxMessageCount = 0,
        StoreSizeBytes = 0,
        MaxStoreBytes = 0,
        InputBufferMessageCount = 0,
        MessagesInPerSecond = 0d,
        MessagesOutPerSecond = 0d
    };
}