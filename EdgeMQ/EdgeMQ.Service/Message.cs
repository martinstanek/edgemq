namespace EdgeMQ.Service;

public sealed record Message
{
    public required ulong Id { get; init; } = 0;

    public required byte[] Payload { get; init; } = [];
}