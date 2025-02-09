namespace EdgeMq.Service.Configuration;

public sealed record EdgeQueueConfiguration
{
    public int EdgeProcessingMessagesDelayMs { get; init; } = 100;

    public required bool IsInMemory { get; init; } = true;

    public required string Name { get; init; } = Constants.DefaultQueueName;

    public required ConstraintViolationMode ConstraintViolationMode { get; init; } = ConstraintViolationMode.Ignore;

    public required InputBufferConfiguration BufferConfiguration { get; init; }

    public required MessageStoreConfiguration StoreConfiguration { get; init; }

    public static EdgeQueueConfiguration Default => new()
    {
        IsInMemory = true,
        Name = Constants.DefaultQueueName,
        ConstraintViolationMode = ConstraintViolationMode.Ignore,
        BufferConfiguration = InputBufferConfiguration.Default,
        StoreConfiguration = MessageStoreConfiguration.Default
    };
}