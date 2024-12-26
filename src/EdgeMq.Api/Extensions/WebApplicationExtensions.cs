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

        todosApi.MapGet("/{name}/stats", async (string name, EdgeQueueHandler handler)
            => Results.Ok((object?)await handler.GetMetricsAsync(name)));

        todosApi.MapGet("/{name}/peek", async (string name, [FromQuery] int batchSize,  EdgeQueueHandler handler)
            => Results.Ok((object?)await handler.PeekAsync(name, batchSize)));

        todosApi.MapGet("/{name}", async (string name, [FromQuery] int batchSize,  EdgeQueueHandler handler)
            => Results.Ok((object?)await handler.DequeueAsync(name, batchSize)));

        todosApi.MapPut("/{name}", async (string name, HttpRequest request, EdgeQueueHandler handler)
            => Results.Ok(await handler.EnqueueAsync(request, name)));

        return webApplication;
    }
}