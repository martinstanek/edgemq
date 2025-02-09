using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Store;
using EdgeMq.Service.Store.FileSystem;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.Service.Store;

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
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message]);
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
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        store.MessageCount.ShouldBe((ulong) 3);
        store.CurrentId.ShouldBe((ulong) 3);
        store.MessageSizeBytes.ShouldBe((ulong) 12);
    }

    [Fact]
    public async Task AddMessages_EmptyList_ReturnsFalse()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();

        await store.InitAsync();
        var added = await store.AddMessagesAsync([]);

        added.ShouldBeFalse();
    }

    [Fact]
    public async Task ReadMessages_InputIsValid_MessagesReturned()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        var messages = await store.ReadMessagesAsync();

        messages.Length.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteMessages_InputIsValid_MessagesDeleted()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var store = context.GetMessageStore();
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        var messages = await store.ReadMessagesAsync();
        var ids = messages.Select(s => s.Id).ToImmutableArray();

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
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        var messages = await store.ReadMessagesAsync(2);
        var ids = messages.Select(s => s.Id).ToImmutableArray();

        await store.DeleteMessagesAsync(ids);

        messages.Length.ShouldBe(2);
        store.MessageCount.ShouldBe((ulong) 1);
        store.CurrentId.ShouldBe((ulong) 3);
        store.MessageSizeBytes.ShouldBe((ulong) 4);
    }

    [Fact]
    public async Task IsFull_CountExceeded_ReturnsTrue()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var config = new MessageStoreConfiguration
        {
            MaxMessageCount = 4
        };
        var store = context.GetMessageStore(config);
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message, message]);

        store.IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFull_CountNotExceeded_ReturnsFalse()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var config = new MessageStoreConfiguration
        {
            MaxMessageCount = 4
        };
        var store = context.GetMessageStore(config);
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        store.IsFull.ShouldBeFalse();
    }

    [Fact]
    public async Task IsFull_SizeExceeded_ReturnsTrue()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var config = new MessageStoreConfiguration
        {
            MaxMessageSizeBytes = 15
        };
        var store = context.GetMessageStore(config);
        var message = new StoreMessage { Payload = "hello" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        store.IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFull_HeadersAccounted_SizeExceeded_ReturnsTrue()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var config = new MessageStoreConfiguration
        {
            MaxMessageSizeBytes = 15
        };
        var store = context.GetMessageStore(config);

        var message = new StoreMessage
        {
            Payload = "hello",
            Headers = new Dictionary<string, string> {{"hello", "hello"}}
        };

        await store.InitAsync();
        await store.AddMessagesAsync([message]);

        store.IsFull.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFull_SizeNotExceeded_ReturnsFalse()
    {
        using var context = new FileSystemMessageStoreTestsContext();
        var config = new MessageStoreConfiguration
        {
            MaxMessageSizeBytes = 100
        };
        var store = context.GetMessageStore(config);
        var message = new StoreMessage { Payload = "test" };

        await store.InitAsync();
        await store.AddMessagesAsync([message, message, message]);

        store.IsFull.ShouldBeFalse();
    }

    private sealed class FileSystemMessageStoreTestsContext : IDisposable
    {
        private string _path = string.Empty;

        public IMessageStore GetMessageStore(MessageStoreConfiguration? configuration = null)
        {
            var config = configuration ?? new MessageStoreConfiguration();

            config = config with
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