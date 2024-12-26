using System;

namespace EdgeMq.Model;

public sealed record QueueRawMessage
{
    public required ulong Id { get; init; }

    public required Guid BatchId { get; init; }

    public required string Payload { get; init; }
}