namespace EdgeMq.Service.Configuration;

public sealed record EdgeQueueConfiguration
{
    public required string Name { get; init; } = Constants.DefaultQueueName;

    public required ConstraintViolationMode ConstraintViolationMode { get; init; } = ConstraintViolationMode.Ignore;

    public required InputBufferConfiguration BufferConfiguration { get; init; }

    public required MessageStoreConfiguration StoreConfiguration { get; init; }
}