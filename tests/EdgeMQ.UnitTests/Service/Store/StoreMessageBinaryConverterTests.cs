using System.Collections.Generic;
using EdgeMq.Service.Store;
using EdgeMq.Service.Store.FileSystem;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service.Store;

public sealed class StoreMessageBinaryConverterTests
{
    [Fact]
    public void FromStoreMessage_ToStoreMessage_HeadersProvided_MessagesEqual()
    {
        const string unicodePayload = "This a unicode text: Žluťoučký kůň";
        var headers = new Dictionary<string, string> { {"key1", "value1" }, {"key2", "čačača"} };

        var storeMessage = new StoreMessage
        {
            Payload = unicodePayload,
            Headers = headers
        };

        var bytes = StoreMessageBinaryConverter.FromStoreMessage(storeMessage);
        var message = StoreMessageBinaryConverter.FromBytes(bytes);

        message.Payload.ShouldBe(storeMessage.Payload);
        message.Headers["key1"].ShouldBe("value1");
        message.Headers["key2"].ShouldBe("čačača");
    }

    [Fact]
    public void FromStoreMessage_ToStoreMessage_HeadersNotProvided_MessagesEqual()
    {
        const string unicodePayload = "This a unicode text: Žluťoučký kůň";

        var storeMessage = new StoreMessage
        {
            Payload = unicodePayload
        };

        var bytes = StoreMessageBinaryConverter.FromStoreMessage(storeMessage);
        var message = StoreMessageBinaryConverter.FromBytes(bytes);

        message.Payload.ShouldBe(storeMessage.Payload);
        message.Headers.ShouldBeEmpty();
    }
}