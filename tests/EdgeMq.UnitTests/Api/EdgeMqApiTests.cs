using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using EdgeMq.Api.Configuration;
using EdgeMq.Model;
using EdgeMq.Client;
using EdgeMq.Client.Exceptions;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.Api;

public sealed class EdgeMqApiTests
{
    [Fact]
    public async Task GetQueues_QueueIsDefined_QueueIsReported()
    {
        const string queueName = "default";

        using var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        var queues = await client.GetQueuesAsync();

        queues.Queues.Count.ShouldBe(1);
        queues.Queues.Single().Name.ShouldBe(queueName);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Peek_NoMessagesAdded_QueueIsEmpty(QueueStoreMode storeMode)
    {
        const string queueName = "default";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(storeMode: storeMode);

        var client = context.GetClient();

        var messages = await client.PeekAsync(queueName, batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName);

        messages.ShouldBeEmpty();
        stats.Name.ShouldBe(queueName);
        stats.MessageCount.ShouldBe((uint)0);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Dequeue_MessagesAdded_QueueIsEmpty(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(storeMode: storeMode);

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

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Dequeue_HeadersUsed_HeadersReturned(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();
        var headers = new Dictionary<string, string> { {"key1", "value1"}, {"key2", "value2"} };

        context.DeclareVariables(storeMode: storeMode);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload, headers);

        await Task.Delay(1000, CancellationToken.None);

        var messages = await client.DequeueAsync(queueName, batchSize: 100);

        messages.First().Headers["key1"].ShouldBe("value1");
        messages.First().Headers["key2"].ShouldBe("value2");
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Dequeue_HeadersUsedMultipleMessages_HeadersReturned(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();
        var headers1 = new Dictionary<string, string> { {"key", "value1"} };
        var headers2 = new Dictionary<string, string> { {"key", "value2"} };
        var headers3 = new Dictionary<string, string> { {"key", "value3"} };

        context.DeclareVariables(storeMode: storeMode);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload, headers1);
        await client.EnqueueAsync(queueName, payload, headers2);
        await client.EnqueueAsync(queueName, payload, headers3);

        await Task.Delay(1000, CancellationToken.None);

        var messages = await client.DequeueAsync(queueName, batchSize: 100);

        messages.Count.ShouldBe(3);
        messages.Any(a => a.Headers["key"].Equals("value1")).ShouldBeTrue();
        messages.Any(a => a.Headers["key"].Equals("value2")).ShouldBeTrue();
        messages.Any(a => a.Headers["key"].Equals("value3")).ShouldBeTrue();
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

        await client.DequeueAsync(queueName, batchSize: 2, timeOut, messages =>
        {
            messages.Count.ShouldBe(2);

            return Task.CompletedTask;

        }, token);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((uint) 1);

        await client.DequeueAsync(queueName, batchSize: 1, timeOut, messages =>
        {
            messages.Count.ShouldBe(1);

            return Task.CompletedTask;

        }, token);

        stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((uint) 0);
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

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Enqueue_PayloadSizeTooBig_ModeSetToFail_Throws(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(storeMode: storeMode, maxPayloadSize: 4, ignoreConstraintsViolation: false);

        var client = context.GetClient();

        var exception = await Should.ThrowAsync<EdgeClientException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Enqueue_PayloadSizeTooBig_ModeSetToIgnore_DoesNotThrow_ReturnsFalse(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(storeMode: storeMode, maxPayloadSize: 4, ignoreConstraintsViolation: true);

        var client = context.GetClient();
        var result = await client.EnqueueAsync(queueName, payload);

        result.Added.ShouldBe(false);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Enqueue_PayloadSizeIsOk_ModeSetToIgnore_DoesNotThrow_ReturnsTrue(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(storeMode: storeMode, maxPayloadSize: 10, ignoreConstraintsViolation: true);

        var client = context.GetClient();
        var result = await client.EnqueueAsync(queueName, payload);

        result.Added.ShouldBe(true);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Enqueue_MaximumCountReached_ModeSetToFail_Throws(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(
            storeMode: storeMode,
            maxMessageCount: 2,
            maxBufferMessageCount: 1,
            ignoreConstraintsViolation: false);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var exception = await Should.ThrowAsync<EdgeClientException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((ulong) 2);
    }

    [Theory]
    [InlineData(QueueStoreMode.InMemory)]
    [InlineData(QueueStoreMode.FileSystem)]
    public async Task Enqueue_MaximumSizeReached_ModeSetToFail_Throws(QueueStoreMode storeMode)
    {
        const string queueName = "default";
        const string payload = "hallo";

        using var context = new EdgeMqApiTestsContext();

        context.DeclareVariables(
            storeMode: storeMode,
            maxMessagesStoreSizeBytes: 10,
            maxMessagesBufferSizeBytes: 5,
            ignoreConstraintsViolation: false);

        var client = context.GetClient();

        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);
        await client.EnqueueAsync(queueName, payload);
        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var exception = await Should.ThrowAsync<EdgeClientException>(() => client.EnqueueAsync(queueName, payload));

        exception.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        await EdgeMqApiTestsContext.PeekUntilPeekedAsync(client, queueName, batchSize: 100);

        var stats = await client.GetMetricsAsync(queueName);

        stats.MessageCount.ShouldBe((ulong) 2);
    }

    private sealed class EdgeMqApiTestsContext : IDisposable
    {
        private const EnvironmentVariableTarget EdgeTarget = EnvironmentVariableTarget.Process;

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
            bool ignoreConstraintsViolation = true,
            QueueStoreMode storeMode = QueueStoreMode.InMemory)
        {
            var constraintMode = ignoreConstraintsViolation
                ? QueueApiConstraintsMode.Ignore
                : QueueApiConstraintsMode.Fail;

            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageCount, maxMessageCount.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageSizeBytes, maxMessagesStoreSizeBytes.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageSizeBytes, maxMessagesBufferSizeBytes.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageCount, maxBufferMessageCount.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxPayloadSizeBytes, maxPayloadSize.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqConstraintsMode, constraintMode.ToString(), EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqStoreMode, storeMode.ToString(), EdgeTarget);
        }

        private static void RemoveVariables()
        {
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageCount, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxMessageSizeBytes, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageSizeBytes, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxBufferMessageCount, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqMaxPayloadSizeBytes, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqConstraintsMode, string.Empty, EdgeTarget);
            Environment.SetEnvironmentVariable(EdgeMqServerConfiguration.EdgeMqStoreMode, string.Empty, EdgeTarget);
        }

        public void Dispose()
        {
            RemoveVariables();
        }
    }
}