## The EdgeMQ

 *... a simple (like really simple) queue designed to have minimal memory footprint and feature set*

![logo](https://github.com/martinstanek/edgemq/blob/develop/misc/logo.svg?raw=true)

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/edgemq)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=52)
[![NuGet](https://img.shields.io/nuget/v/Awitec.EdgeMq.Client.svg)](https://www.nuget.org/packages/Awitec.EdgeMq.Client) 
![Docker Image Version](https://img.shields.io/docker/v/awitec/edgemq)

### Enqueue

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
var edgeMqClient = new EdgeMqClient(httpClient);
await edgeMqClient.EnqueueAsync("test-queue", "hello world!");
```

### Dequeue

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
var edgeMqClient = new EdgeMqClient(httpClient);
await edgeMqClient.DequeueAsync("test-queue", batch, TimeSpan.FromSeconds(1), messages =>
{
    foreach (var message in messages)
    {
        Console.WriteLine(message.Payload);
    }

    return Task.CompletedTask;

}, CancellationToken.None);
```

### Compose

```yml
services:

  edgemq:
    hostname: edgemq
    container_name: edgemq
    image: "awitec/edgemq:0.0.70-arm64"
    ports:
      - 2323:2323
    environment:
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

Happy Queueing,\
Martin