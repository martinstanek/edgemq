namespace EdgeMq.Service.Model;

public sealed record Metrics
{
    public required ulong MessageCount { get; init; }

    public required ulong MaxMessageCount { get; init; }

    public required ulong MessageSizeBytes { get; init; }

    public required ulong MaxMessageSizeBytes { get; init; }

    public required ulong BufferMessageCount { get; init; }

    public required ulong MaxBufferMessageCount { get; init; }

    public required ulong BufferMessageSizeBytes { get; init; }

    public required ulong MaxBufferMessageSizeBytes { get; init; }

    public required ulong CurrentCurrentId { get; init; }

    public required ulong ProcessedMessages { get; init; }

    public required double MessagesInPerSecond { get; init; }

    public required double MessagesOutPerSecond { get; init; }
}