namespace EdgeMq.Service.Configuration;

public sealed record InputBufferConfiguration
{
    public ulong MaxPayloadSizeBytes { get; init; } = 10 * 1024 * 1024;

    public ulong MaxMessageCount { get; init; } = 100000;

    public ulong MaxMessageSizeBytes { get; init; } = 1024 * 1024;

    public ConstraintViolationMode Mode { get; init; } = ConstraintViolationMode.Ignore;
}