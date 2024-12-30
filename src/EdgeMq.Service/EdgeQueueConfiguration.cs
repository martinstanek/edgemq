using EdgeMq.Service.Input;
using EdgeMq.Service.Store;

namespace EdgeMq.Service;

public sealed record EdgeQueueConfiguration
{
    public required string Name { get; init; }

    public required InputBufferConfiguration BufferConfiguration { get; init; }

    public required MessageStoreConfiguration StoreConfiguration { get; init; }
}