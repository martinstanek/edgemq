using EdgeMQ.Service.Input;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests;

public sealed class InputBufferTests
{
    [Fact]
    public async Task Test()
    {
        var config = new InputBufferConfiguration();
        var buffer = new InputBuffer(config);
        var token = CancellationToken.None;
        var message = "hello"u8.ToArray();

        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);
        await buffer.AddAsync(message, token);

        var messages = await buffer.ReadAllAsync(token);

        messages.Count.ShouldBe(3);
    }
}