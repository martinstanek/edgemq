using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using EdgeMq.Api.Configuration;
using EdgeMq.Api.Handlers;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/queue");

        api.MapGet("/", async ([FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.GetQueuesAsync()));

        api.MapGet("/{name}", async (string name, [FromQuery] int batchSize, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.DequeueAsync(name, batchSize)));

        api.MapGet("/{name}/stats", async (string name, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.GetMetricsAsync(name)));

        api.MapGet("/{name}/peek", async (string name, [FromQuery] int batchSize, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.PeekAsync(name, batchSize)));

        api.MapPatch("/{name}", async (string name, [FromQuery] Guid batchId, [FromServices] IEdgeQueueHandler handler) =>
        {
            await handler.AcknowledgeAsync(name, batchId);

            return Results.NoContent();
        });

        api.MapPut("/{name}", async (string name, HttpRequest request, [FromServices] IEdgeQueueHandler handler, [FromServices] EdgeMqServerConfiguration config) =>
        {
            var added = await handler.EnqueueAsync(request, name);

            return !added && config.ConstraintsMode == QueueApiConstraintsMode.Fail
                ? Results.UnprocessableEntity()
                : Results.NoContent();
        });

        return webApplication;
    }
}