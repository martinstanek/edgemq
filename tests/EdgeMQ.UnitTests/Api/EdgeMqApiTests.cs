using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using EdgeMq.Api.Configuration;
using EdgeMq.Model;
using EdgeMq.Client;
using Refit;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Api;

public sealed class EdgeMqApiTests
{
    [Fact]
    public async Task GetQueues_QueueIsDefined_QueueIsReported()
    {
        const string queueName = "default";

        using var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        var queues = await client.GetQueuesAsync();

        queues.Count.ShouldBe(1);
        queues.First().Name.ShouldBe(queueName);
    }

    [Fact]
    public async Task Peek_NoMessagesAdded_QueueIsEmpty()
    {
        const string queueName = "default";

        using var context = new EdgeMqApiTestsContext();
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

        using var context = new EdgeMqApiTestsContext();
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

        using var context = new EdgeMqApiTestsContext();
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

        using var context = new EdgeMqApiTestsContext();
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

        using var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await client.EnqueueAsync(queueName, payload);

        var messages = await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        await client.AcknowledgeAsync(queueName, messages.First().BatchId);

        var stats = await client.GetMetricsAsync(queueName);

        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint)0);
    }

    [Fact]
    public async Task Enqueue_PayloadSizeTooBig_ModeSetToFail_Throws()
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(maxPayloadSize: 4, ignoreConstraintsViolation: false);

        var client = context.GetClient();

        var exception = await Should.ThrowAsync<ApiException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Enqueue_MaximumCountReached_ModeSetToFail_Throws()
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(maxMessageCount: 2, maxBufferMessageCount: 1, ignoreConstraintsViolation: false);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var exception = await Should.ThrowAsync<ApiException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((ulong) 2);
    }

    [Fact]
    public async Task Enqueue_MaximumSizeReached_ModeSetToFail_Throws()
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(maxMessagesStoreSizeBytes: 10, maxMessagesBufferSizeBytes: 5, ignoreConstraintsViolation: false);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var exception = await Should.ThrowAsync<ApiException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((ulong) 2);
    }

    private sealed class EdgeMqApiTestsContext : IDisposable
    {
        private const EnvironmentVariableTarget Target = EnvironmentVariableTarget.Process;

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

        internal void DeclareVariables(
            byte maxMessageCount = 10,
            byte maxMessagesStoreSizeBytes = 200,
            byte maxBufferMessageCount = 5,
            byte maxMessagesBufferSizeBytes = 100,
            byte maxPayloadSize = 10,
            bool ignoreConstraintsViolation = true)
        {
            var constraintMode = ignoreConstraintsViolation
                ? QueueApiConstraintsMode.Ignore
                : QueueApiConstraintsMode.Fail;

            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageCount, maxMessageCount.ToString(), Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageSizeBytes, maxMessagesStoreSizeBytes.ToString(), Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageSizeBytes, maxMessagesBufferSizeBytes.ToString(), Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageCount, maxBufferMessageCount.ToString(), Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxPayloadSizeBytes, maxPayloadSize.ToString(), Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqConstraintsMode, constraintMode.ToString(), Target);
        }

        private static void RemoveVariables()
        {
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageCount, string.Empty, Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageSizeBytes, string.Empty, Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageSizeBytes, string.Empty, Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageCount, string.Empty, Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxPayloadSizeBytes, string.Empty, Target);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqConstraintsMode, string.Empty, Target);
        }

        public void Dispose()
        {
            RemoveVariables();
        }
    }
}