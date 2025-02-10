using System.Threading.Tasks;
using EdgeMq.TestContainer;
using Shouldly;
using Xunit;

namespace EdgeMq.UnitTests.TestContainer;

public sealed class EdgeQueueTestContainerTests
{
    [Fact]
    public async Task GetClient_ContainerIsCreated_ClientReturned()
    {
        await using var testContainer = new EdgeQueueTestContainer();

        if (!await testContainer.IsTestable())
        {
            return;
        }

        var client = await testContainer.GetClientAsync();
        var queues = await client.GetQueuesAsync();

        queues.Queues.ShouldNotBeEmpty();
    }
}