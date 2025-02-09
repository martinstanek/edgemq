namespace EdgeMq.Client;

public sealed record EdgeMqClientConfiguration
{
    public required string ApiKey { get; init; } = string.Empty;

    public static EdgeMqClientConfiguration Empty => new EdgeMqClientConfiguration
    {
        ApiKey = string.Empty
    };
}