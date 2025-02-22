using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Exceptions;
using EdgeMq.Service.Input;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.Service.Input;

public sealed class InputBufferTests
{
    [Fact]
    public async Task Add_MessagesAdded()
    {
        var config = new InputBufferConfiguration();
        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello";

        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);

        buffer.MessageMessageCount.ShouldBe((ulong) 3);
        buffer.MessageMessagesSize.ShouldBe((ulong) 15);

        var messages = await buffer.ReadAllAsync(token);

        messages.Length.ShouldBe(3);
        buffer.MessageMessageCount.ShouldBe((ulong) 0);
        buffer.MessageMessagesSize.ShouldBe((ulong) 0);
    }

    [Fact]
    public async Task ReadAllAsync_NoMessages_ReturnsEmptyList()
    {
        var config = new InputBufferConfiguration();
        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello";

        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);

        buffer.MessageMessageCount.ShouldBe((ulong) 3);
        buffer.MessageMessagesSize.ShouldBe((ulong) 15);

        var messages1 = await buffer.ReadAllAsync(token);
        var messages2 = await buffer.ReadAllAsync(token);

        messages1.ShouldNotBeEmpty();
        messages2.ShouldBeEmpty();
    }

    [Fact]
    public async Task Add_MaximumMessagesReached_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxMessageCount = 2
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello";

        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);

        buffer.MessageMessageCount.ShouldBe((ulong) 2);
        buffer.MessageMessagesSize.ShouldBe((ulong) 10);
    }

    [Fact]
    public async Task Add_PayloadTooBig_NotAdded_Throws()
    {
        var config = new InputBufferConfiguration
        {
            MaxMessageCount = 2,
            MaxPayloadSizeBytes = 5,
            Mode = ConstraintViolationMode.ThrowException
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello-this-is-too-big";

        await Should.ThrowAsync<EdgeQueueInputException>(() => buffer.TryAddAsync(message, headers, token));
    }

    [Fact]
    public async Task Add_PayloadTooBig_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxMessageCount = 2,
            MaxPayloadSizeBytes = 5,
            Mode = ConstraintViolationMode.Ignore
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello-this-is-too-big";

        var added = await buffer.TryAddAsync(message, headers, token);

        added.ShouldBeFalse();
    }

    [Fact]
    public async Task Add_MaximumCumulativePayloadSizeReached_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxMessageSizeBytes = 10
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello";

        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);

        buffer.MessageMessageCount.ShouldBe((ulong) 2);
        buffer.MessageMessagesSize.ShouldBe((ulong) 10);
    }

    [Fact]
    public async Task Add_MaximumPayloadSizeReached_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxPayloadSizeBytes = 4
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var headers = ReadOnlyDictionary<string, string>.Empty;
        var message = "hello";

        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);
        await buffer.TryAddAsync(message, headers, token);

        buffer.MessageMessageCount.ShouldBe((ulong) 0);
        buffer.MessageMessagesSize.ShouldBe((ulong) 0);
    }
}