using EdgeMQ.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests;

public sealed class MessageStoreTests
{
    [Fact]
    public async Task AddMessages_MessagesAdded()
    {
        var store = new InMemoryMessageStore();
        var message1 = new Message { Id = 1, Payload = "hello"u8.ToArray() };
        var message2 = new Message { Id = 2, Payload = "hello"u8.ToArray() };

        await store.AddMessagesAsync([message1, message2]);

        store.MessageCount.ShouldBe(2);
        store.MessageSizeBytes.ShouldBe(10);
    }

    [Fact]
    public async Task DeleteMessages_MessagesDeleted()
    {
        var store = new InMemoryMessageStore();
        var message1 = new Message { Id = 1, Payload = "hello"u8.ToArray() };
        var message2 = new Message { Id = 2, Payload = "hello"u8.ToArray() };

        await store.AddMessagesAsync([message1, message2]);

        store.MessageCount.ShouldBe(2);
        store.MessageSizeBytes.ShouldBe(10);

        await store.DeleteMessagesAsync([message1.Id]);

        store.MessageCount.ShouldBe(1);
        store.MessageSizeBytes.ShouldBe(5);

        await store.DeleteMessagesAsync([message2.Id]);

        store.MessageCount.ShouldBe(0);
        store.MessageSizeBytes.ShouldBe(0);
    }
}