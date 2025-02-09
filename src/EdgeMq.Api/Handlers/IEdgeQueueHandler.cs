using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Handlers;

public interface IEdgeQueueHandler
{
    Task<IResult> AcknowledgeAsync(string queueName, Guid batchId);

    Task<IResult> EnqueueAsync(HttpRequest request, string queueName);

    Task<IResult> GetMetricsAsync(string queueName);

    Task<IResult> DequeueAsync(string queueName, int batchSize);

    Task<IResult> PeekAsync(string queueName, int batchSize);

    Task<IResult> GetQueuesAsync();
}