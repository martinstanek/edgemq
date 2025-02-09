using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public interface IEdgeQueueHandler
{
    Task<IResult> AcknowledgeAsync(string queueName, string apiKey, Guid batchId);

    Task<IResult> EnqueueAsync(HttpRequest request, string queueName, string apiKey);

    Task<IResult> GetMetricsAsync(string queueName, string apiKey);

    Task<IResult> DequeueAsync(string queueName, string apiKey, int batchSize);

    Task<IResult> PeekAsync(string queueName, string apiKey, int batchSize);

    Task<IResult> GetQueuesAsync(string apiKey);
}