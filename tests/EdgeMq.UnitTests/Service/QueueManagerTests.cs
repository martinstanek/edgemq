using EdgeMq.Service;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Exceptions;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.Service;

public sealed class QueueManagerTests
{
    [Theory]
    [InlineData("aa#bbb")]
    [InlineData("čřť")]
    [InlineData("_aaa_")]
    public void AddQueue_NameIsInvalid_Throws(string queueName)
    {
        var manager = new QueueManager();
        var config = EdgeQueueConfiguration.Default with { Name = queueName };

        Should.Throw<EdgeQueueException>(() => manager.AddQueue(config));
    }

    [Fact]
    public void AddQueue_NameIsAlreadyAdded_Ignores()
    {
        var manager = new QueueManager();

        manager.AddQueue(EdgeQueueConfiguration.Default with { Name = "test1"});
        manager.AddQueue(EdgeQueueConfiguration.Default with { Name = "test2"});
        manager.AddQueue(EdgeQueueConfiguration.Default with { Name = "test2"});

        manager.QueueNames.Length.ShouldBe(2);
        manager["test1"].Name.ShouldBe("test1");
        manager["test2"].Name.ShouldBe("test2");
    }
}