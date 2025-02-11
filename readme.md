## The EdgeMq

 *... a simple (like seriously and naively simple), lightweight queue designed to have a minimal memory footprint*

Multi producer, single consumer & plenty of space for improvements.

![logo](https://github.com/martinstanek/edgemq/blob/develop/misc/logo.svg?raw=true)

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/edgemq)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=52)
[![NuGet](https://img.shields.io/nuget/v/Awitec.EdgeMq.Client.svg)](https://www.nuget.org/packages/Awitec.EdgeMq.Client)
[![NuGet](https://img.shields.io/nuget/v/Awitec.EdgeMq.TestContainer.svg)](https://www.nuget.org/packages/Awitec.EdgeMq.TestContainer)
![Docker Image Version](https://img.shields.io/docker/v/awitec/edgemq)

![logo](https://github.com/martinstanek/edgemq/blob/develop/misc/ui.png?raw=true)

### Producer

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
var config = new EdgeMqClientConfiguration { ApiKey = "123" };
var edgeMqClient = new EdgeMqClient(httpClient, config);

await edgeMqClient.EnqueueAsync("test-queue", "hello world!");
```

### Consumer

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
var config = new EdgeMqClientConfiguration { ApiKey = "123" };
var edgeMqClient = new EdgeMqClient(httpClient, config);

await edgeMqClient.DequeueAsync("test-queue", batch, timeOut: TimeSpan.FromSeconds(1), messages =>
{
    foreach (var message in messages)
    {
        Console.WriteLine(message.Payload);
    }

    return Task.CompletedTask;

}, CancellationToken.None);
```

### Server

```
UI -> http://localhost:2323/
OpenApi -> http://localhost:2323/openapi/v1.json
Scalar -> http://localhost:2323/scalar/
API -> http://localhost:2323/v1/queues
```
### Docker

Available Docker Tags: https://hub.docker.com/repository/docker/awitec/edgemq/tags

### Compose

```yml
services:

  edgemq:
    hostname: edgemq
    container_name: edgemq
    image: "awitec/edgemq:latest-arm64"
    ports:
      - 2323:2323
    environment:
      - EDGEMQ_APIKEY=123
      - EDGEMQ_PATH=/data/queues
      - EDGEMQ_QUEUES=test-queue
      - EDGEMQ_STOREMODE=FileSystem
      - EDGEMQ_CONSTRAINTSMODE=Ignore
      - EDGEMQ_MAXCOUNT=100000
      - EDGEMQ_MAXSIZEBYTES=10485760
      - EDGEMQ_MAXBUFFERCOUNT=1000
      - EDGEMQ_MAXBUFFERSIZEBYTES=1048576
      - EDGEMQ_PAYLOADSIZEBYTES=1024
      - EDGEMQ_BATCHSIZE=100
    volumes:
      - edgemqdata:/data
      - /etc/localtime:/etc/localtime:ro
    restart: unless-stopped

volumes:
  edgemqdata:
```

When no env. vars provided, the server will default to InMemory mode without any API key.

### Testing

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


Happy Queueing,\
Martin