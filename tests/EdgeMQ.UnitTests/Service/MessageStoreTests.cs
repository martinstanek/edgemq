using System.Linq;
using System.Threading.Tasks;
using EdgeMq.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service;

public sealed class MessageStoreTests
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
}