using System;
using EdgeMq.Api.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureEndpoints(this WebApplication webApplication)
    {
        var todosApi = webApplication.MapGroup("/queue");

        todosApi.MapGet("/{name}", async (string name, [FromQuery] int batchSize, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.DequeueAsync(name, batchSize)));

        todosApi.MapGet("/{name}/stats", async (string name, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.GetMetricsAsync(name)));

        todosApi.MapGet("/{name}/peek", async (string name, [FromQuery] int batchSize, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.PeekAsync(name, batchSize)));

        todosApi.MapPut("/{name}", async (string name, HttpRequest request, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.EnqueueAsync(request, name)));

        todosApi.MapPatch("/{name}", async (string name, [FromQuery] Guid batchId, [FromServices] IEdgeQueueHandler handler)
            => Results.Ok(await handler.AcknowledgeAsync(name, batchId)));

        return webApplication;
    }
}