using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EdgeMQ.Service;

public interface IEdgeMq
{
    Task QueueAsync(byte[] payload, CancellationToken cancellationToken);

    Task DeQueueAsync(uint batchSize, Func<Task, IReadOnlyCollection<Message>> process, TimeSpan timeOut, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken);

    Task AckAsync(string batchId);

    void Start(CancellationToken cancellationToken);

    void Stop();
}