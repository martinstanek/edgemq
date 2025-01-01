using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Store;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service.Store;

public sealed class FileSystemMessageStoreTests
{
    [Fact]
    public async Task Init_DirectoryIsEmpty_ValuesAreZero()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();

        store.MessageCount.ShouldBe((ulong) 0);
        store.CurrentId.ShouldBe((ulong) 0);
        store.MessageSizeBytes.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task Init_DirectoryIsNotEmpty_ValuesAreNotZero()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        await store.AddMessagesAsync(["test"]);
        await store.InitAsync();

        store.MessageCount.ShouldBe((ulong) 1);
        store.CurrentId.ShouldBe((ulong) 1);
        store.MessageSizeBytes.ShouldBe((ulong) 4);
    }

    [Fact]
    public async Task AddMessages_InputIsValid_MessagesPersisted()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        await store.AddMessagesAsync(["test", "test", "test"]);

        store.MessageCount.ShouldBe((ulong) 3);
        store.CurrentId.ShouldBe((ulong) 3);
        store.MessageSizeBytes.ShouldBe((ulong) 12);
    }

    [Fact]
    public async Task ReadMessages_InputIsValid_MessagesReturned()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        await store.AddMessagesAsync(["test", "test", "test"]);

        var messages = await store.ReadMessagesAsync();

        messages.Count.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteMessages_InputIsValid_MessagesDeleted()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        await store.AddMessagesAsync(["test", "test", "test"]);

        var messages = await store.ReadMessagesAsync();
        var ids = messages.Select(s => s.Id).ToList();

        await store.DeleteMessagesAsync(ids);

        store.MessageCount.ShouldBe((ulong) 0);
        store.CurrentId.ShouldBe((ulong) 0);
        store.MessageSizeBytes.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task DeleteMessages_SomeIdsProvided_MessagesDeleted()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        await store.AddMessagesAsync(["test", "test", "test"]);

        var messages = await store.ReadMessagesAsync(2);
        var ids = messages.Select(s => s.Id).ToList();

        await store.DeleteMessagesAsync(ids);

        messages.Count.ShouldBe(2);
        store.MessageCount.ShouldBe((ulong) 1);
        store.CurrentId.ShouldBe((ulong) 3);
        store.MessageSizeBytes.ShouldBe((ulong) 4);
    }

    private sealed class FileSystemMessageStoreTestsContext : IDisposable
    {
        private string _path = string.Empty;

        public IMessageStore GetMessageStore()
        {
            var config = new MessageStoreConfiguration
            {
                Path = "./test-queue"
            };

            _path = config.Path;

            return new FileSystemMessageStore(config);
        }

        public void Dispose()
        {
            if (Directory.Exists(_path))
            {
                Directory.Delete(_path, recursive: true);
            }
        }
    }
}