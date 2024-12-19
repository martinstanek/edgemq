using System;

namespace EdgeMQ.Service.Input;

public sealed record BufferMessage
{
    public long EnqueuedTicks { get; } = DateTime.UtcNow.Ticks;

    public required byte[] Payload { get; init; } = [];
}