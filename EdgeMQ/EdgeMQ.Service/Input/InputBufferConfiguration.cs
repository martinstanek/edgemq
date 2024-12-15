namespace EdgeMQ.Service.Input;

public sealed record InputBufferConfiguration
{
    public long MaxBufferSizeBytes { get; init; } = 10 * 1024 * 1024;

    public int MaxMessageCount { get; init; } = 100_000;

    public int MaxMessageSizeBytes { get; init; } = 1024 * 1024;

    public ConstraintViolationMode Mode { get; init; } = ConstraintViolationMode.Ignore;
}