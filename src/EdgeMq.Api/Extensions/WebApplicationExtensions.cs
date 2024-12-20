using EdgeMq.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureEndpoints(this WebApplication webApplication)
    {
        var todosApi = webApplication.MapGroup("/queue");

        todosApi.MapGet("/{name}", (string name) =>
        {
            return Results.Ok(new QueueMetrics
            {
                Name = name,
                MessageCount = 12
            });
        });
   
        return webApplication;
    }
}