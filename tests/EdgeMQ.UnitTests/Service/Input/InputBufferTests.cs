using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Input;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service.Input;

public sealed class InputBufferTests
{
    [Fact]
    public async Task Add_MessagesAdded()
    {
        var config = new InputBufferConfiguration();
        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var message = "hello";

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessageCount.ShouldBe((ulong) 3);
        buffer.MessageSizeBytes.ShouldBe((ulong) 15);

        var messages = await buffer.ReadAllAsync(token);

        messages.Count.ShouldBe(3);
        buffer.MessageCount.ShouldBe((ulong) 0);
        buffer.MessageSizeBytes.ShouldBe((ulong) 0);
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
        var message = "hello";

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessageCount.ShouldBe((ulong) 2);
        buffer.MessageSizeBytes.ShouldBe((ulong) 10);
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
        var message = "hello";

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessageCount.ShouldBe((ulong) 2);
        buffer.MessageSizeBytes.ShouldBe((ulong) 10);
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
        var message = "hello";

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessageCount.ShouldBe((ulong) 0);
        buffer.MessageSizeBytes.ShouldBe((ulong) 0);
    }
}