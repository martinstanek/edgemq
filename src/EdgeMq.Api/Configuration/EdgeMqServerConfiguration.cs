using System;
using System.Collections.Generic;
using EdgeMq.Service.Configuration;

namespace EdgeMq.Api.Configuration;

public sealed record EdgeMqServerConfiguration
{
    public required QueueStoreMode StoreMode { get; init; } = QueueStoreMode.InMemory;

    public required QueueApiConstraintsMode ConstraintsMode { get; init; } = QueueApiConstraintsMode.Ignore;

    public required string Path { get; init; } = Constants.DefaultRootPath;

    public required IReadOnlyCollection<string> Queues { get; init; } = [ Constants.DefaultQueueName ];

    public ulong MaxMessageCount { get; init; } = Constants.DefaultMaxMessageCount;

    public ulong MaxMessageSizeBytes { get; init; } = Constants.DefaultMaxMessageSizeBytes;

    public ulong MaxBufferMessageCount { get; init; } = Constants.DefaultBufferMaxMessageCount;

    public ulong MaxBufferMessageSizeBytes { get; init; } = Constants.DefaultBufferMaxMessageSizeBytes;

    public ulong MaxPayloadSizeBytes { get; init; } = Constants.DefaultMaxPayloadSizeBytes;

    public uint DefaultBatchSize { get; init; } = Constants.DefaultBatchSize;

    public static EdgeMqServerConfiguration ReadFromEnvironment()
    {
        var defaultConfig = Default;

        return new EdgeMqServerConfiguration
        {
            Path = EnvReader.GetEnvironmentValue("EDGEMQ_PATH", defaultConfig.Path),
            Queues = EnvReader.GetEnvironmentValue("EDGEMQ_QUEUES", Constants.DefaultQueueName).Split(','),
            DefaultBatchSize = EnvReader.GetEnvironmentValue("EDGEMQ_BATCHSIZE", defaultConfig.DefaultBatchSize),
            MaxMessageCount = EnvReader.GetEnvironmentValue("EDGEMQ_MAXCOUNT", defaultConfig.MaxMessageCount),
            MaxMessageSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_MAXSIZEBYTES", defaultConfig.MaxMessageSizeBytes),
            MaxPayloadSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_PAYLOADSIZEBYTES", defaultConfig.MaxPayloadSizeBytes),
            MaxBufferMessageCount = EnvReader.GetEnvironmentValue("EDGEMQ_MAXBUFFERCOUNT", defaultConfig.MaxBufferMessageCount),
            MaxBufferMessageSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_MAXBUFFERSIZEBYTES", defaultConfig.MaxBufferMessageSizeBytes),
            StoreMode = Enum.Parse<QueueStoreMode>(EnvReader.GetEnvironmentValue("EDGEMQ_STOREMODE", defaultConfig.StoreMode.ToString())),
            ConstraintsMode = Enum.Parse<QueueApiConstraintsMode>(EnvReader.GetEnvironmentValue("EDGEMQ_CONSTRAINTSMODE", defaultConfig.ConstraintsMode.ToString()))
        };
    }

    public static EdgeMqServerConfiguration Default => new()
    {
        StoreMode = QueueStoreMode.InMemory,
        ConstraintsMode = QueueApiConstraintsMode.Ignore,
        Path = string.Empty,
        Queues = []
    };
}