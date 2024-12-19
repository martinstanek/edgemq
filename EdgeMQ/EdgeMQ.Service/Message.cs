using System;

namespace EdgeMQ.Service;

public sealed record Message
{
    public Guid BatchId { get; init; } = Guid.Empty;

    public required ulong Id { get; init; } = 0;

    public required byte[] Payload { get; init; } = [];
}