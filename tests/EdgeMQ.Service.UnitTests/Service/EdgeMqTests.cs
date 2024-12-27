using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeMQ.Service.Input;
using EdgeMQ.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests.Service;

public sealed class EdgeMqTests
{
    [Fact]
    public async Task Peek_MessagesPeeked_QueueNotAltered()
    {
        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var payload = "test";

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

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
        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var payload = "test";

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        var messages = await EdgeMqTestsContext.PeekUntilPeekedAsync(queue, 10, token);

        var batchId = messages.First().BatchId;

        await queue.AcknowledgeAsync(batchId, token);

        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
        queue.Stop();
    }

    [Fact]
    public async Task DirectDeque_MessagesAcknowledged_QueueAltered()
    {
        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var payload = "test";

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        var messages = await queue.DeQueueAsync(10, token);

        messages.Count.ShouldBe(3);
        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
        queue.Stop();
    }

    [Fact]
    public async Task Deque_MessagesAcknowledged_QueueAltered()
    {
        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var payload = "test";
        var messages = Array.Empty<Message>();

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        await queue.DeQueueAsync(batchSize: 10, TimeSpan.FromSeconds(1), m =>
        {
            messages = m.ToArray();

            return Task.CompletedTask;

        }, token);

        messages.Length.ShouldBe(3);
        queue.MessageSizeBytes.ShouldBe((ulong) 0);
        queue.MessageCount.ShouldBe((ulong) 0);
        queue.Stop();
    }

    private sealed class EdgeMqTestsContext
    {
        internal IEdgeMq GetQueue()
        {
            var queueConfig = new EdgeQueueConfiguration { Name = "test" };
            var storeConfig = new MessageStoreConfiguration();
            var bufferConfig = new InputBufferConfiguration();
            var buffer = new InputBuffer(bufferConfig);
            var store = new InMemoryMessageStore(storeConfig);
            var queue = new EdgeMq(buffer, store, queueConfig);

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