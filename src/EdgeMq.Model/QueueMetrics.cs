namespace EdgeMq.Model;

public sealed record QueueMetrics
{
    public required string Name { get; init; } = string.Empty;
    
    public required ulong MessageCount { get; init; }
}