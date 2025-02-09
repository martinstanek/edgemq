using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Exceptions;
using EdgeMq.Service.Input;
using EdgeMq.Service.Model;
using EdgeMq.Service.Store.InMemory;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.Service;

public sealed class QueueTests
{
    [Fact]
    public async Task Peek_MessagesPeeked_QueueNotAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await EdgeMqTestsContext.PeekUntilPeekedAsync(queue, 10, token);

        messages.Length.ShouldBe(3);
        messages.All(a => a.BatchId == messages.First().BatchId).ShouldBeTrue();
        queue.Metrics.BufferMessageCount.ShouldBe((ulong) 0);
        queue.Metrics.BufferMessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.CurrentCurrentId.ShouldBe((ulong) 3);
        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 12);
        queue.Metrics.MessageCount.ShouldBe((ulong) 3);
    }

    [Fact]
    public async Task Acknowledge_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await EdgeMqTestsContext.PeekUntilPeekedAsync(queue, 10, token);

        var batchId = messages.First().BatchId;

        await queue.AcknowledgeAsync(batchId, token);

        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task DirectDeque_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await queue.DequeueAsync(10, token);

        messages.Length.ShouldBe(3);
        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var messages = Array.Empty<Message>();

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        await queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Length.ShouldBe(3);
            messages = m.ToArray();

            return Task.CompletedTask;

        }, token);

        messages.Length.ShouldBe(3);
        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_Twice_OnlyFirstReturnsMessages()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        await queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Length.ShouldBe(3);
            return Task.CompletedTask;

        }, token);

        await queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Length.ShouldBe(0);
            return Task.CompletedTask;

        }, token);

        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_Parallel_DoesNotThrow()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await queue.EnqueueAsync(payload, token);
        await Task.Delay(1000, token);

        var t1 = queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);
        var t2 = queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);
        var t3 = queue.DequeueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);

        await Task.WhenAll(t1, t2, t3);

        queue.Metrics.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.Metrics.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public void AddQueue_NameIsInvalid_UnicodeChar_ThrowsEdgeQueueException()
    {
        var context = new EdgeMqTestsContext();

        Should.Throw<EdgeQueueException>(() => context.GetQueue("ŽŽŽ-čkkk"));
    }

    [Fact]
    public void AddQueue_NameIsInvalid_SpecialChar_ThrowsEdgeQueueException()
    {
        var context = new EdgeMqTestsContext();

        Should.Throw<EdgeQueueException>(() => context.GetQueue("aaaaa=bbb"));
    }

    [Fact]
    public void AddQueue_NameIsInvalid_TooSmall_ThrowsEdgeQueueException()
    {
        var context = new EdgeMqTestsContext();

        Should.Throw<EdgeQueueException>(() => context.GetQueue("a"));
    }

    [Fact]
    public void AddQueue_NameIsInvalid_TooBig_ThrowsEdgeQueueException()
    {
        var context = new EdgeMqTestsContext();

        Should.Throw<EdgeQueueException>(() => context.GetQueue("1234567890123456789012345678901234567890"));
    }

    private sealed class EdgeMqTestsContext
    {
        internal IEdgeQueue GetQueue(string name = "test")
        {
            var storeConfig = new MessageStoreConfiguration();
            var bufferConfig = new InputBufferConfiguration();
            var queueConfig = new EdgeQueueConfiguration
            {
                Name = name,
                IsInMemory = true,
                BufferConfiguration = bufferConfig,
                StoreConfiguration = storeConfig,
                ConstraintViolationMode = ConstraintViolationMode.Ignore
            };
            var buffer = new InputBuffer(bufferConfig);
            var store = new InMemoryMessageStore(storeConfig);
            var queue = new EdgeQueue(buffer, store, queueConfig);

            return queue;
        }

        internal static async Task<ImmutableArray<Message>> PeekUntilPeekedAsync(
            IEdgeQueue queue,
            uint batchSize,
            CancellationToken token)
        {
            var messages = new List<Message>();

            while (messages.Count == 0)
            {
                messages = (await queue.PeekAsync(batchSize, token)).ToList();
            }

            return messages.ToImmutableArray();
        }
    }
}