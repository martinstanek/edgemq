using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using EdgeMq.Api.Handlers;
using EdgeMq.Model;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    private const string EdgeApiKeyHeader = "X-Api-Key";

    public static WebApplication UseApiEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/v1/queues");

        api.MapGet("/", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromServices] IEdgeQueueHandler handler)
                => await handler.GetQueuesAsync(apiKey ?? string.Empty))
            .Produces<ImmutableArray<Queue>>();

        api.MapGet("/{name}", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromRoute] string name,
                    [FromQuery] int batchSize,
                    [FromServices] IEdgeQueueHandler handler)
                => await handler.DequeueAsync(name, apiKey ?? string.Empty, batchSize))
            .Produces<ImmutableArray<QueueRawMessage>>();

        api.MapGet("/{name}/stats", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromRoute] string name,
                    [FromServices] IEdgeQueueHandler handler)
                => await handler.GetMetricsAsync(name, apiKey ?? string.Empty))
            .Produces<QueueMetrics>();

        api.MapGet("/{name}/peek", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromRoute] string name,
                    [FromQuery] int batchSize,
                    [FromServices] IEdgeQueueHandler handler)
                => await handler.PeekAsync(name, apiKey ?? string.Empty, batchSize))
            .Produces<ImmutableArray<QueueRawMessage>>();

        api.MapPatch("/{name}", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromRoute] string name,
                    [FromQuery] Guid batchId,
                    [FromServices] IEdgeQueueHandler handler)
                => await handler.AcknowledgeAsync(name, apiKey ?? string.Empty, batchId))
            .Produces(StatusCodes.Status204NoContent);

        api.MapPost("/{name}", async (
                    [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                    [FromRoute] string name,
                    [FromServices] IEdgeQueueHandler handler,
                    HttpRequest request)
                => await handler.EnqueueAsync(request, name, apiKey ?? string.Empty))
            .Produces<QueueEnqueueResult>();

        return webApplication;
    }
}