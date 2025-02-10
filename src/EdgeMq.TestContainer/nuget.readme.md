## The EdgeMQ Client

*... simple native .NET docker wrapper for the EdgeMQ queue docker imager*

```csharp
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
```

For more info, examples and source please visit: https://github.com/martinstanek/edgemq
