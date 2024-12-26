using System.Linq;
using System.Threading.Tasks;
using EdgeMq.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EdgeMQ.Service.UnitTests.Api;

public sealed class EdgeMqApiTests
{
    [Fact]
    public async Task Peek_NoMessagesAdded_QueueIsEmpty()
    {
        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        var messages = await client.PeekAsync(queueName: "test-queue", batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName: "test-queue");

        messages.ShouldBeEmpty();
        stats.Name.ShouldBe("test-queue");
        stats.MessageCount.ShouldBe((uint)0);
    }

    [Fact]
    public async Task Dequeue_MessagesAdded_QueueIsEmpty()
    {
        const string payload = "hallo";

        var context = new EdgeMqApiTestsContext();
        var client = context.GetClient();

        await client.EnqueueAsync("test-queue", payload);
        var messages = await client.DequeueAsync(queueName: "test-queue", batchSize: 100);
        var stats = await client.GetMetricsAsync(queueName: "test-queue");

        messages.Count.ShouldBe(1);
        messages.First().Payload.ShouldBe(payload);
        stats.Name.ShouldBe("test-queue");
        stats.MessageCount.ShouldBe((uint)0);
    }

    private sealed class EdgeMqApiTestsContext
    {
        internal IEdgeMqClient GetClient()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                    });
                });

            var httpClient = application.CreateClient();

            return new EdgeMqClient(httpClient);
        }
    }
}
