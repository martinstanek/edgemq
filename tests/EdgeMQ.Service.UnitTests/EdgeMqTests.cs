using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeMQ.Service.Input;
using EdgeMQ.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests;

public sealed class EdgeMqTests
{
    [Fact]
    public async Task Peek_MessagesPeeked_QueueNotAltered()
    {
        using var queue = new EdgeMqTestsContext().GetQueue();
        var token = CancellationToken.None;
        var payload = "test"u8.ToArray();

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        var messages = await queue.PeekAsync(batchSize: 10, token);

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
        var payload = "test"u8.ToArray();

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        var messages = await queue.PeekAsync(batchSize: 10, token);
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
        var payload = "test"u8.ToArray();

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
        var payload = "test"u8.ToArray();
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
            var storeConfig = new MessageStoreConfiguration();
            var bufferConfig = new InputBufferConfiguration();
            var buffer = new InputBuffer(bufferConfig);
            var store = new InMemoryMessageStore(storeConfig);
            var queue = new EdgeMq(buffer, store);

            return queue;
        }
    }
}