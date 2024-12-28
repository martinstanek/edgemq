using System;

namespace EdgeMq.Service.Input;

public sealed record BufferMessage
{
    public long EnqueuedTicks { get; } = DateTime.UtcNow.Ticks;

    public required string Payload { get; init; } = string.Empty;
}