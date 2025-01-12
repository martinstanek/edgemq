using System;
using System.Collections.Generic;
using EdgeMq.Service.Configuration;

namespace EdgeMq.Api.Configuration;

public sealed record EdgeMqServerConfiguration
{
    public const string EdgeMqPath = "EDGEMQ_PATH";
    public const string EdgeMqQueues = "EDGEMQ_QUEUES";
    public const string EdgeMqStoreMode = "EDGEMQ_STOREMODE";
    public const string EdgeMqConstraintsMode = "EDGEMQ_CONSTRAINTSMODE";
    public const string EdgeMqDefaultBatchSize = "EDGEMQ_BATCHSIZE";
    public const string EdgeMqMaxMessageCount = "EDGEMQ_MAXCOUNT";
    public const string EdgeMqMaxMessageSizeBytes = "EDGEMQ_MAXSIZEBYTES";
    public const string EdgeMqMaxPayloadSizeBytes = "EDGEMQ_PAYLOADSIZEBYTES";
    public const string EdgeMqMaxBufferMessageCount = "EDGEMQ_MAXBUFFERCOUNT";
    public const string EdgeMqMaxBufferMessageSizeBytes = "EDGEMQ_MAXBUFFERSIZEBYTES";

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
            Path = EnvReader.GetEnvironmentValue(EdgeMqPath, defaultConfig.Path),
            Queues = EnvReader.GetEnvironmentValue(EdgeMqQueues, Constants.DefaultQueueName).Split(','),
            DefaultBatchSize = EnvReader.GetEnvironmentValue(EdgeMqDefaultBatchSize, defaultConfig.DefaultBatchSize),
            MaxMessageCount = EnvReader.GetEnvironmentValue(EdgeMqMaxMessageCount, defaultConfig.MaxMessageCount),
            MaxMessageSizeBytes = EnvReader.GetEnvironmentValue(EdgeMqMaxMessageSizeBytes, defaultConfig.MaxMessageSizeBytes),
            MaxPayloadSizeBytes = EnvReader.GetEnvironmentValue(EdgeMqMaxPayloadSizeBytes, defaultConfig.MaxPayloadSizeBytes),
            MaxBufferMessageCount = EnvReader.GetEnvironmentValue(EdgeMqMaxBufferMessageCount, defaultConfig.MaxBufferMessageCount),
            MaxBufferMessageSizeBytes = EnvReader.GetEnvironmentValue(EdgeMqMaxBufferMessageSizeBytes, defaultConfig.MaxBufferMessageSizeBytes),
            StoreMode = Enum.Parse<QueueStoreMode>(EnvReader.GetEnvironmentValue(EdgeMqStoreMode, defaultConfig.StoreMode.ToString())),
            ConstraintsMode = Enum.Parse<QueueApiConstraintsMode>(EnvReader.GetEnvironmentValue(EdgeMqConstraintsMode, defaultConfig.ConstraintsMode.ToString()))
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