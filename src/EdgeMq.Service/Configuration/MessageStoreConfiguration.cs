namespace EdgeMq.Service.Configuration;

public sealed record MessageStoreConfiguration
{
    public ulong MaxMessageCount { get; init; } = 1000000;

    public ulong MaxMessageSizeBytes { get; init; } = 536870912;

    public uint DefaultBatchSize { get; init; } = 100;

    public string Path { get; init; } = "./queues/default";
}