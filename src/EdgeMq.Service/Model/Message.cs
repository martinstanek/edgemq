using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeMq.Service.Model;

public sealed record Message
{
    public Guid BatchId { get; init; } = Guid.Empty;

    public required ulong Id { get; init; }

    public required string Payload { get; init; } = string.Empty;

    public required IReadOnlyDictionary<string, string> Headers { get; init; }

    public ulong GetMessageSizeBytes()
    {
        var result = (ulong) Encoding.UTF8.GetByteCount(Payload);

        foreach (var header in Headers)
        {
            result += (ulong) Encoding.UTF8.GetByteCount(header.Key);
            result += (ulong) Encoding.UTF8.GetByteCount(header.Value);
        }

        return result;
    }
}