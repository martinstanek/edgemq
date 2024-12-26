using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EdgeMQ.Service;

public interface IEdgeMq : IDisposable
{
    Task QueueAsync(string payload, CancellationToken cancellationToken);

    Task DeQueueAsync(uint batchSize, TimeSpan timeOut, Func<IReadOnlyCollection<Message>, Task> process, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> DeQueueAsync(uint batchSize, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken);

    Task AcknowledgeAsync(Guid batchId, CancellationToken cancellationToken);

    void Start(CancellationToken cancellationToken);

    void Stop();

    public string Name { get; }

    public ulong MessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong BufferMessageCount { get; }

    public ulong BufferMessageSizeBytes { get; }

    public ulong MaxMessageCount { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong CurrentCurrentId { get; }
}