namespace EdgeMq.Service.Store;

public sealed record MessageStoreConfiguration
{
    public ulong MaxMessageCount { get; init; } = 100_000;

    public ulong MaxMessageSizeBytes { get; init; } = 536870912;

    public uint DefaultBatchSize { get; init; } = 100;

    public string Path { get; init; } = string.Empty;
}