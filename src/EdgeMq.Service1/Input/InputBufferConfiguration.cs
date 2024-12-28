namespace EdgeMq.Service1.Input;

public sealed record InputBufferConfiguration
{
    public ulong MaxBufferSizeBytes { get; init; } = 10 * 1024 * 1024;

    public uint MaxMessageCount { get; init; } = 100_000;

    public uint MaxMessageSizeBytes { get; init; } = 1024 * 1024;

    public ConstraintViolationMode Mode { get; init; } = ConstraintViolationMode.Ignore;
}