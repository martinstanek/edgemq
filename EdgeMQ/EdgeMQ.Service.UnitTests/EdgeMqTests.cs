using EdgeMQ.Service.Input;
using EdgeMQ.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests;

public sealed class EdgeMqTests
{
    [Fact]
    public async Task MessagesQueued_MessagesDeQueued()
    {
        var payload = "test"u8.ToArray();
        var storeConfig = new MessageStoreConfiguration();
        var bufferConfig = new InputBufferConfiguration();
        var token = CancellationToken.None;
        var buffer = new InputBuffer(bufferConfig);
        var store = new InMemoryMessageStore(storeConfig);
        var queue = new EdgeMq(buffer, store);

        queue.Start(CancellationToken.None);

        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);
        await queue.QueueAsync(payload, token);

        var messages = await queue.PeekAsync(batchSize: 10, token);

        messages.Count.ShouldBe(3);
        messages.All(a => a.BatchId == messages.First().BatchId).ShouldBeTrue();

        queue.Stop();
    }
}