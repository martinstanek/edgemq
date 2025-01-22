using System.Collections.Generic;

namespace EdgeMq.Service.Store;

public struct StoreMessage
{
    public string Payload { get; init; }

    public IReadOnlyDictionary<string, string> Headers { get; init; }
}