using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeMq.Api.Configuration;

public sealed record EdgeMqServerConfiguration
{
    public required QueueStoreMode Mode { get; init; } = QueueStoreMode.InMemory;

    public required string Path { get; init; } = "./queues";

    public required IReadOnlyCollection<string> Queues { get; init; } = [ "default" ];

    public ulong MaxMessageCount { get; init; } = 1000000;

    public ulong MaxMessageSizeBytes { get; init; } = 536870912;

    public ulong MaxBufferMessageCount { get; init; } = 1000;

    public ulong MaxBufferMessageSizeBytes { get; init; } = 1048576;

    public uint DefaultBatchSize { get; init; } = 100;

    public static EdgeMqServerConfiguration Empty => new()
    {
        Mode = QueueStoreMode.InMemory,
        Path = string.Empty,
        Queues = []
    };

    public static EdgeMqServerConfiguration ReadFromEnvironment()
    {
        var defaultConfig = Empty;
        var path = Environment.GetEnvironmentVariable("EDGEMQ_PATH") ?? defaultConfig.Path;
        var queues = Environment.GetEnvironmentVariable("EDGEMQ_QUEUES") ?? defaultConfig.Queues.First();
        var mode = Enum.TryParse<QueueStoreMode>(Environment.GetEnvironmentVariable("EDGEMQ_MODE"), out var envMode)
            ? envMode
            : QueueStoreMode.InMemory;
        var maxCount = ulong.TryParse(Environment.GetEnvironmentVariable("EDGEMQ_MAXCOUNT"), out var envMaxCount)
            ? envMaxCount
            : defaultConfig.MaxMessageCount;
        var maxSize = ulong.TryParse(Environment.GetEnvironmentVariable("EDGEMQ_MAXSIZEBYTES"), out var envMaxSize)
            ? envMaxSize
            : defaultConfig.MaxMessageSizeBytes;
        var maxBufferCount = ulong.TryParse(Environment.GetEnvironmentVariable("EDGEMQ_MAXBUFFERCOUNT"), out var envMaxBufferCount)
            ? envMaxBufferCount
            : defaultConfig.MaxBufferMessageCount;
        var maxBufferSize = ulong.TryParse(Environment.GetEnvironmentVariable("EDGEMQ_MAXBUFFERSIZEBYTES"), out var envMaxBufferSize)
            ? envMaxBufferSize
            : defaultConfig.MaxBufferMessageSizeBytes;
        var batchSize = uint.TryParse(Environment.GetEnvironmentVariable("EDGEMQ_BATCHSIZE"), out var envBatchSize)
            ? envBatchSize
            : defaultConfig.DefaultBatchSize;

        return new EdgeMqServerConfiguration
        {
            Mode = mode,
            Path = path,
            Queues = queues.Split(','),
            MaxMessageCount = maxCount,
            MaxMessageSizeBytes = maxSize,
            MaxBufferMessageCount = maxBufferCount,
            MaxBufferMessageSizeBytes = maxBufferSize,
            DefaultBatchSize = batchSize
        };
    }
}