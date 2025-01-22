using System;
using System.Collections.Generic;

namespace EdgeMq.Service;

public sealed record Message
{
    public Guid BatchId { get; init; } = Guid.Empty;

    public required ulong Id { get; init; }

    public required string Payload { get; init; } = string.Empty;

    public required IReadOnlyDictionary<string, string> Headers { get; init; }
}