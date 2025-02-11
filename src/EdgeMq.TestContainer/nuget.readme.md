## The EdgeMq.TestContainer

*The docker test container wrapper used for the integration tests.*

The test container NuGet can be used for the derived integration tests, (the default architecture is arm64) ie.:

```csharp
[Fact]
public async Task GetClient_ContainerIsCreated_ClientReturned()
{
    await using var testContainer = new EdgeQueueTestContainer();

    if (!await testContainer.IsTestable())
    {
        return;
    }

    var client = await testContainer.GetClientAsync(
          testContainerName: "my-test-container", 
          testQueueName: "my-test-queue", 
          EdgeQueueTestContainer.ImageArchitecture.Amd64);
     
    var queues = await client.GetQueuesAsync();

    queues.Queues.ShouldNotBeEmpty();
}
```

For more info, examples and source please visit: https://github.com/martinstanek/edgemq