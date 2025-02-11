namespace EdgeMq.Service.Configuration;

public sealed record MessageStoreConfiguration
{
    public ulong MaxMessageCount { get; init; } = Constants.DefaultMaxMessageCount;

    public ulong MaxMessageSizeBytes { get; init; } = Constants.DefaultMaxMessageSizeBytes;

    public uint DefaultBatchSize { get; init; } = Constants.DefaultBatchSize;

    public string Path { get; init; } = Constants.DefaultQueuePath;

    public static MessageStoreConfiguration Default => new();
}