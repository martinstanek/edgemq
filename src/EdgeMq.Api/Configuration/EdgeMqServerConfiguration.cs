using System;
using System.Collections.Generic;

namespace EdgeMq.Api.Configuration;

public sealed record EdgeMqServerConfiguration
{
    public required QueueStoreMode Mode { get; init; } = QueueStoreMode.InMemory;

    public required string Path { get; init; } = string.Empty;

    public required IReadOnlyCollection<string> Queues { get; init; } = [];

    public ulong MaxMessageCount { get; init; } = 100_000;

    public ulong MaxMessageSizeBytes { get; init; } = 536870912;

    public uint DefaultBatchSize { get; init; } = 100;

    public static EdgeMqServerConfiguration ReadFromEnvironment()
    {
        var path = Environment.GetEnvironmentVariable("EDGEMQ_PATH") ?? "./queues";
        var queues = Environment.GetEnvironmentVariable("EDGEMQ_QUEUES") ?? "default";
        var mode = Enum.TryParse<QueueStoreMode>(Environment.GetEnvironmentVariable("EDGEMQ_MODE"), out var envMode)
            ? envMode
            : QueueStoreMode.InMemory;

        return new EdgeMqServerConfiguration
        {
            Mode = mode,
            Path = path,
            Queues = queues.Split(',')
        };
    }
}