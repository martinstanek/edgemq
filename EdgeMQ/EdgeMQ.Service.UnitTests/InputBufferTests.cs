using EdgeMQ.Service.Input;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests;

public sealed class InputBufferTests
{
    [Fact]
    public async Task AddAndRead_MessagesAdded_Removed()
    {
        var config = new InputBufferConfiguration();
        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var message = "hello"u8.ToArray();

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessagesCount.ShouldBe(3);
        buffer.PayloadSize.ShouldBe(15);

        var messages = await buffer.ReadAllAsync(token);

        messages.Count.ShouldBe(3);
        buffer.MessagesCount.ShouldBe(0);
        buffer.PayloadSize.ShouldBe(0);
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
        var message = "hello"u8.ToArray();

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessagesCount.ShouldBe(2);
        buffer.PayloadSize.ShouldBe(10);
    }

    [Fact]
    public async Task Add_MaximumCumulativePayloadSizeReached_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxBufferSizeBytes = 10
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var message = "hello"u8.ToArray();

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessagesCount.ShouldBe(2);
        buffer.PayloadSize.ShouldBe(10);
    }

    [Fact]
    public async Task Add_MaximumPayloadSizeReached_NotAdded()
    {
        var config = new InputBufferConfiguration
        {
            MaxMessageSizeBytes = 4
        };

        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var message = "hello"u8.ToArray();

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        buffer.MessagesCount.ShouldBe(0);
        buffer.PayloadSize.ShouldBe(0);
    }
}