namespace EdgeMq.Service.Configuration;

public static class Constants
{
    public const string DefaultQueueName = "default";
    public const string DefaultRootPath = "./queues";
    public const string DefaultQueuePath = "./queues/default";
    public const uint DefaultBatchSize = 100;
    public const ulong DefaultMaxPayloadSizeBytes = 2097152; // 2MB
    public const ulong DefaultBufferMaxMessageCount  = 10_000;
    public const ulong DefaultBufferMaxMessageSizeBytes = 10485760; // 10MB
    public const ulong DefaultMaxMessageCount = 1_000_000;
    public const ulong DefaultMaxMessageSizeBytes = 536870912; // 512MB
}