using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service;

public sealed class EdgeMqTests
{
    [Fact]
    public async Task Peek_MessagesPeeked_QueueNotAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await EdgeMqTestsContext.PeekUntilPeekedAsync(queue, 10, token);

        messages.Count.ShouldBe(3);
        messages.All(a => a.BatchId == messages.First().BatchId).ShouldBeTrue();
        queue.BufferMessageCount.ShouldBe((ulong) 0);
        queue.BufferMessageSizeBytes.ShouldBe((ulong) 0);
        queue.CurrentCurrentId.ShouldBe((ulong) 3);
        queue.MessageSizeBytes.ShouldBe((ulong) 12);
        queue.MessageCount.ShouldBe((ulong) 3);
    }

    [Fact]
    public async Task Acknowledge_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await EdgeMqTestsContext.PeekUntilPeekedAsync(queue, 10, token);

        var batchId = messages.First().BatchId;

        await queue.AcknowledgeAsync(batchId, token);

        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task DirectDeque_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        var messages = await queue.DeQueueAsync(10, token);

        messages.Count.ShouldBe(3);
        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_MessagesAcknowledged_QueueAltered()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var messages = Array.Empty<Message>();

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        await queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Count.ShouldBe(3);
            messages = m.ToArray();

            return Task.CompletedTask;

        }, token);

        messages.Length.ShouldBe(3);
        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_Twice_OnlyFirstReturnsMessages()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        await queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Count.ShouldBe(3);
            return Task.CompletedTask;

        }, token);

        await queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            m.Count.ShouldBe(0);
            return Task.CompletedTask;

        }, token);

        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Deque_Parallel_DoesNotThrow()
    {
        const string payload = "test";

        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await Task.Delay(1000, token);

        var t1 = queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);
        var t2 = queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);
        var t3 = queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), _ => Task.CompletedTask, token);

        await Task.WhenAll(t1, t2, t3);

        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
    }

    private sealed class EdgeMqTestsContext
    {
        internal IEdgeMq GetQueue()
        {
            var storeConfig = new MessageStoreConfiguration();
            var bufferConfig = new InputBufferConfiguration();
            var queueConfig = new EdgeQueueConfiguration
            {
                Name = "test",
                BufferConfiguration = bufferConfig,
                StoreConfiguration = storeConfig
            };
            var buffer = new InputBuffer(bufferConfig);
            var store = new InMemoryMessageStore(storeConfig);
            var queue = new EdgeMq.Service.EdgeMq(buffer, store, queueConfig);

            return queue;
        }

        internal static async Task<IReadOnlyCollection<Message>> PeekUntilPeekedAsync(
            IEdgeMq queue,
            uint batchSize,
            CancellationToken token)
        {
            var messages = new List<Message>();

            while (messages.Count == 0)
            {
                messages = (await queue.PeekAsync(batchSize, token)).ToList();
            }

            return messages;
        }
    }
}