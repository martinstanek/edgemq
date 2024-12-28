namespace EdgeMq.Model;

public sealed record Queue
{
    public required string Name { get; init; } = string.Empty;

    public required string Mode { get; init; } = string.Empty;

    public required QueueMetrics Metrics { get; init; } = QueueMetrics.Empty;
}