using System.Collections.Generic;

namespace EdgeMq.Service.Input;

public sealed record BufferMessage
{
    public required string Payload { get; init; } = string.Empty;

    public required IReadOnlyDictionary<string, string> Headers { get; init; }
}