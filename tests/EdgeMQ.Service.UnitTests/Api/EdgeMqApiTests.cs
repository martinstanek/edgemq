using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests.Api;

public sealed class EdgeMqApiTests
{
    [Fact]
    public async Task Peek_NoMessagesAdded_QueueIsEmpty()
    {
        const string queueName = "test-queue";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        var messages = await client.PeekAsync(queueName, batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName);

        messages.ShouldBeEmpty();
        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint)0);
    }

    [Fact]
    public async Task Dequeue_MessagesAdded_QueueIsEmpty()
    {
        const string queueName = "test-queue";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        var messages = await client.DequeueAsync(queueName, batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName);

        messages.Count.ShouldBe(1);
        messages.First().Payload.ShouldBe(payload);
        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint)0);
    }

    [Fact]
    public async Task DequeueByProcessing_MessagesAdded_QueueIsEmpty()
    {
        const string queueName = "test-queue";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();
        var timeOut = TimeSpan.FromSeconds(5);
        var token = CancellationToken.None;

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);
        await client.DequeueAsync(queueName, batchSize: 100, timeOut, messages =>
        {
            messages.Count.ShouldBe(3);

            return Task.CompletedTask;

        }, token);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((uint) 0);

        await client.DequeueAsync(queueName, batchSize: 100, timeOut, messages =>
        {
            messages.Count.ShouldBe(0);

            return Task.CompletedTask;

        }, token);
    }

    [Fact]
    public async Task Acknowledge_MessagesAdded_QueueIsEmpty()
    {
        const string queueName = "test-queue";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);

        var messages = await client.PeekAsync(queueName, batchSize: 100);

        await client.AcknowledgeAsync(queueName, messages.First().BatchId);

        var stats = await client.GetMetricsAsync(queueName);

        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint)0);
    }

    private sealed class EdgeMqApiTestsContext
    {
        internal IEdgeMqClient GetClient()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                    });
                });

            var httpClient = application.CreateClient();

            return new EdgeMqClient(httpClient);
        }
    }
}
