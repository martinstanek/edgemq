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

    Task AckAsync(Guid batchId);

    void Start(CancellationToken cancellationToken);

    void Stop();

    public ulong MessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong BufferMessageCount { get; }

    public ulong BufferMessageSizeBytes { get; }

    public ulong MaxMessageCount { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong CurrentCurrentId { get; }
}