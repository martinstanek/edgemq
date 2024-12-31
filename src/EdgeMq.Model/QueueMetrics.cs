namespace EdgeMq.Model;

public sealed record QueueMetrics
{
    public required string Name { get; init; } = string.Empty;
    
    public required ulong MessageCount { get; init; }

    public required ulong MaxMessageCount { get; init; }

    public required ulong MessagesSizeBytes { get; init; }

    public required ulong MaxMessagesSizeBytes { get; init; }

    public required ulong BufferMessageCount { get; init; }

    public required ulong MaxBufferMessageCount { get; init; }

    public required ulong BufferMessagesSizeBytes { get; init; }

    public required ulong MaxBufferMessagesSizeBytes { get; init; }

    public required double MessagesInPerSecond { get; init; }

    public required double MessagesOutPerSecond { get; init; }

    public required double BufferMessagesSizePressure { get; init; }

    public required double BufferMessageCountPressure { get; init; }

    public required double MessageCountPressure { get; init; }

    public required double MessagesSizePressure { get; init; }

    public static QueueMetrics Empty => new()
    {
        Name = string.Empty,
        MessageCount = 0,
        MaxMessageCount = 0,
        MessagesSizeBytes = 0,
        MaxMessagesSizeBytes = 0,
        BufferMessageCount = 0,
        MaxBufferMessageCount = 0,
        BufferMessagesSizeBytes = 0,
        MaxBufferMessagesSizeBytes = 0,
        MessagesInPerSecond = 0d,
        MessagesOutPerSecond = 0d,
        BufferMessageCountPressure = 0d,
        BufferMessagesSizePressure = 0d,
        MessageCountPressure = 0d,
        MessagesSizePressure = 0d
    };
}