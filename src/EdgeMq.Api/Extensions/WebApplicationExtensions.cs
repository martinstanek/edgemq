using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using EdgeMq.Model;
using EdgeMQ.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureEndpoints(this WebApplication webApplication)
    {
            var todosApi = webApplication.MapGroup("/queue");

        todosApi.MapGet("/", (IEdgeMq queue) =>
        {
            return Results.Ok(new QueueMetrics
            {
                Name = queue.Name,
                MessageCount = queue.MessageCount
            });
        });

        todosApi.MapGet("/{name}", async (string name, IEdgeMq queue) =>
        {
            var messages = await queue.DeQueueAsync(batchSize: 100, CancellationToken.None);

            var result = messages.Select(s => new QueueRawMessage
            {
                Id = s.Id,
                BatchId = s.BatchId,
                PayloadBase64 = Convert.ToBase64String(s.Payload)
            });

            return Results.Ok(result.ToArray());
        });

        todosApi.MapPut("/{name}", async (HttpRequest request, IEdgeMq queue) =>
        {
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false);

            var rawContent = await reader.ReadToEndAsync();
            var bytes = Convert.FromBase64String(rawContent);
            await queue.QueueAsync(bytes, CancellationToken.None);

            return Results.Ok(new QueueMetrics
            {
                Name = queue.Name,
                MessageCount = queue.MessageCount
            });
        });
   
        return webApplication;
    }
}