using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using EdgeMq.Api.Handlers;

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
            => await handler.GetQueuesAsync());

        api.MapGet("/{name}", async (
                [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                [FromRoute] string name,
                [FromQuery] int batchSize,
                [FromServices] IEdgeQueueHandler handler)
            => await handler.DequeueAsync(name, batchSize));

        api.MapGet("/{name}/stats", async (
                [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                [FromRoute] string name,
                [FromServices] IEdgeQueueHandler handler)
            => await handler.GetMetricsAsync(name));

        api.MapGet("/{name}/peek", async (
                [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                [FromRoute] string name,
                [FromQuery] int batchSize,
                [FromServices] IEdgeQueueHandler handler)
            => await handler.PeekAsync(name, batchSize));

        api.MapPatch("/{name}", async (
                [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                [FromRoute] string name,
                [FromQuery] Guid batchId,
                [FromServices] IEdgeQueueHandler handler)
            => await handler.AcknowledgeAsync(name, batchId));

        api.MapPost("/{name}", async (
                [FromHeader(Name = EdgeApiKeyHeader)] string? apiKey,
                [FromRoute] string name,
                [FromServices] IEdgeQueueHandler handler,
                HttpRequest request)
            => await handler.EnqueueAsync(request, name));

        return webApplication;
    }
}