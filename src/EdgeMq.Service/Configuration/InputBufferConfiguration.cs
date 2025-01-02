namespace EdgeMq.Service.Configuration;

public sealed record InputBufferConfiguration
{
    public ulong MaxPayloadSizeBytes { get; init; } = Constants.DefaultMaxPayloadSizeBytes;

    public ulong MaxMessageCount { get; init; } = Constants.DefaultBufferMaxMessageCount;

    public ulong MaxMessageSizeBytes { get; init; } = Constants.DefaultBufferMaxMessageSizeBytes;

    public ConstraintViolationMode Mode { get; init; } = ConstraintViolationMode.Ignore;
}