using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using EdgeMq.Model;
using EdgeMq.Client;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Api;

public sealed class EdgeMqApiTests
{
    [Fact]
    public async Task GetQueues_QueueIsDefined_QueueIsReported()
    {
        const string queueName = "default";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        var queues = await client.GetQueuesAsync();

        queues.Count.ShouldBe(1);
        queues.First().Name.ShouldBe(queueName);
    }

    [Fact]
    public async Task Peek_NoMessagesAdded_QueueIsEmpty()
    {
        const string queueName = "default";

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
        const string queueName = "default";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);

        await Task.Delay(1000, CancellationToken.None);

        var messages = await client.DequeueAsync(queueName, batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName);

        messages.Count.ShouldBe(1);
        messages.First().Payload.ShouldBe(payload);
        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint) 0);
    }

    [Fact]
    public async Task DequeueByProcessing_MessagesAdded_QueueIsEmpty()
    {
        const string queueName = "default";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();
        var timeOut = TimeSpan.FromSeconds(5);
        var token = CancellationToken.None;

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);

        await Task.Delay(1000, token);

        var stats = await client.DequeueAsync(queueName, batchSize: 100, timeOut, messages =>
        {
            messages.Count.ShouldBe(3);

            return Task.CompletedTask;

        }, token);

        stats.MessageCount.ShouldBe((uint) 3);

        stats = await client.DequeueAsync(queueName, batchSize: 100, timeOut, messages =>
        {
            messages.Count.ShouldBe(0);

            return Task.CompletedTask;

        }, token);

        stats.MessageCount.ShouldBe((uint) 0);
    }

    [Fact]
    public async Task DequeueByProcessing_BatchSizeDefined_BatchSizeRespected()
    {
        const string queueName = "default";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();
        var timeOut = TimeSpan.FromSeconds(5);
        var token = CancellationToken.None;

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);

        await Task.Delay(1000, token);

        var stats = await client.DequeueAsync(queueName, batchSize: 2, timeOut, messages =>
        {
            messages.Count.ShouldBe(2);

            return Task.CompletedTask;

        }, token);

        stats.MessageCount.ShouldBe((uint) 3);

        stats = await client.DequeueAsync(queueName, batchSize: 1, timeOut, messages =>
        {
            messages.Count.ShouldBe(1);

            return Task.CompletedTask;

        }, token);

        stats.MessageCount.ShouldBe((uint) 1);
    }

    [Fact]
    public async Task Acknowledge_MessagesAdded_QueueIsEmpty()
    {
        const string queueName = "default";
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);

        var messages = await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

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
                .WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); });

            var httpClient = application.CreateClient();


            return new EdgeMqClient(httpClient);
        }

        internal static async Task<IReadOnlyCollection<QueueRawMessage>> PeekUntilPeekedAsync(
            IEdgeMqClient queue,
            string queueName,
            int batchSize)
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var messages = new List<QueueRawMessage>();

            while (messages.Count == 0 | !tokenSource.IsCancellationRequested)
            {
                messages = (await queue.PeekAsync(queueName, batchSize)).ToList();
            }

            return messages;
        }
    }
}
