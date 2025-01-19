## The EdgeMQ

 *... a simple (like really simple) queue designed to have minimal memory footprint and feature set*

![logo](https://github.com/martinstanek/bucket/blob/main/misc/logo.png?raw=true)

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/edgemq)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=52) \
.. NuGet \
.. Docker

### Enqueue

```csharp
public static viod ChangeMe()
{}
```

### Dequeue

```csharp
public static viod ChangeMe()
{}
```

### Compose


```yml
services:

  edgemq:
    hostname: edgemq
    container_name: edgemq
    image: "awitec.azurecr.io/edgemq:0.0.70-arm64"
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