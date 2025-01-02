using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EdgeMq.Service;

public interface IEdgeMq : IDisposable
{
    Task<bool> EnqueueAsync(string payload, CancellationToken cancellationToken);

    Task DequeueAsync(uint batchSize, TimeSpan timeOut, Func<IReadOnlyCollection<Message>, Task> process, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> DequeueAsync(uint batchSize, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken);

    Task AcknowledgeAsync(Guid batchId, CancellationToken cancellationToken);

    void Start(CancellationToken cancellationToken);

    void Stop();

    public string Name { get; }

    public ulong MessageCount { get; }

    public ulong MaxMessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong BufferMessageCount { get; }

    public ulong MaxBufferMessageCount { get; }

    public ulong BufferMessageSizeBytes { get; }

    public ulong MaxBufferMessageSizeBytes { get; }

    public ulong CurrentCurrentId { get; }

    public double MessagesInPerSecond { get; }

    public double MessagesOutPerSecond { get; }
}