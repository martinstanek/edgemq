using System;
using EdgeMq.Api.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        api.MapPut("/{name}", async (string name, HttpRequest request, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.EnqueueAsync(request, name)));

        api.MapPatch("/{name}", async (string name, [FromQuery] Guid batchId, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.AcknowledgeAsync(name, batchId)));

        return webApplication;
    }
}