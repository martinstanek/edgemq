using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EdgeMq.Service.Store;

public struct StoreMessage
{
    public StoreMessage() { }

    public string Payload { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Headers { get; init; } =  ReadOnlyDictionary<string, string>.Empty;
}