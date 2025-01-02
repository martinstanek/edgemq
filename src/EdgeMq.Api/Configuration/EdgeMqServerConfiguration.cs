using System;
using System.Collections.Generic;

namespace EdgeMq.Api.Configuration;

public sealed record EdgeMqServerConfiguration
{
    public const string DefaultPath = "./queues";
    public const string DefaultQueue = "default";

    public required QueueStoreMode Mode { get; init; } = QueueStoreMode.InMemory;

    public required string Path { get; init; } = DefaultPath;

    public required IReadOnlyCollection<string> Queues { get; init; } = [ DefaultQueue ];

    public ulong MaxMessageCount { get; init; } = 1000000;

    public ulong MaxMessageSizeBytes { get; init; } = 536870912;

    public ulong MaxBufferMessageCount { get; init; } = 1000;

    public ulong MaxBufferMessageSizeBytes { get; init; } = 1048576;

    public uint MaxPayloadSizeBytes { get; init; } = 1024 * 1024;

    public uint DefaultBatchSize { get; init; } = 100;

    public static EdgeMqServerConfiguration ReadFromEnvironment()
    {
        var defaultConfig = Empty;

        return new EdgeMqServerConfiguration
        {
            Mode = Enum.Parse<QueueStoreMode>(EnvReader.GetEnvironmentValue("EDGEMQ_MODE", QueueStoreMode.InMemory.ToString())),
            Path = EnvReader.GetEnvironmentValue("EDGEMQ_PATH", defaultConfig.Path),
            Queues = EnvReader.GetEnvironmentValue("EDGEMQ_QUEUES", DefaultQueue).Split(','),
            DefaultBatchSize = EnvReader.GetEnvironmentValue("EDGEMQ_BATCHSIZE", defaultConfig.DefaultBatchSize),
            MaxMessageCount = EnvReader.GetEnvironmentValue("EDGEMQ_MAXCOUNT", defaultConfig.MaxMessageCount),
            MaxMessageSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_MAXSIZEBYTES", defaultConfig.MaxMessageSizeBytes),
            MaxPayloadSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_PAYLOADSIZEBYTES", defaultConfig.MaxPayloadSizeBytes),
            MaxBufferMessageCount = EnvReader.GetEnvironmentValue("EDGEMQ_MAXBUFFERCOUNT", defaultConfig.MaxBufferMessageCount),
            MaxBufferMessageSizeBytes = EnvReader.GetEnvironmentValue("EDGEMQ_MAXBUFFERSIZEBYTES", defaultConfig.MaxBufferMessageSizeBytes)
        };
    }

    public static EdgeMqServerConfiguration Empty => new()
    {
        Mode = QueueStoreMode.InMemory,
        Path = string.Empty,
        Queues = []
    };
}