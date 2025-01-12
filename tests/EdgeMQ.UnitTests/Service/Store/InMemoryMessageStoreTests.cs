using System.Linq;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service.Store;

public sealed class InMemoryMessageStoreTests
{
    [Fact]
    public async Task AddMessages_MessagesAdded()
    {
        var config = new MessageStoreConfiguration();
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload]);

        store.MessageCount.ShouldBe((ulong) 2);
        store.MessageSizeBytes.ShouldBe((ulong) 10);
    }

    [Fact]
    public async Task DeleteMessages_MessagesDeleted()
    {
        var config = new MessageStoreConfiguration();
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload]);

        var messages = await store.ReadMessagesAsync();

        messages.Count.ShouldBe(2);

        await store.DeleteMessagesAsync([messages.First().Id]);

        store.MessageCount.ShouldBe((ulong) 1);
        store.MessageSizeBytes.ShouldBe((ulong) 5);

        await store.DeleteMessagesAsync([messages.Last().Id]);

        store.MessageCount.ShouldBe((ulong) 0);
        store.MessageSizeBytes.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task IsFull_CountExceeded_ReturnsTrue()
    {
        var config = new MessageStoreConfiguration
        {
            MaxMessageCount = 4
        };
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload, payload, payload]);

        store.IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFull_CountNotExceeded_ReturnsFalse()
    {
        var config = new MessageStoreConfiguration
        {
            MaxMessageCount = 4
        };
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload, payload]);

        store.IsFull.ShouldBeFalse();
    }

    [Fact]
    public async Task IsFull_SizeExceeded_ReturnsTrue()
    {
        var config = new MessageStoreConfiguration
        {
            MaxMessageSizeBytes = 15
        };
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload, payload]);

        store.IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFull_SizeNotExceeded_ReturnsFalse()
    {
        var config = new MessageStoreConfiguration
        {
            MaxMessageSizeBytes = 100
        };
        var store = new InMemoryMessageStore(config);
        var payload = "hello";

        await store.AddMessagesAsync([payload, payload, payload]);

        store.IsFull.ShouldBeFalse();
    }
}