using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EdgeMq.Service.Model;

namespace EdgeMq.Service;

public interface IEdgeMq : IDisposable
{
    Task<bool> EnqueueAsync(string payload, CancellationToken cancellationToken);

    Task<bool> EnqueueAsync(string payload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken);

    Task DequeueAsync(uint batchSize, TimeSpan timeOut, Func<IReadOnlyCollection<Message>, Task> process, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> DequeueAsync(uint batchSize, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken);

    Task AcknowledgeAsync(Guid batchId, CancellationToken cancellationToken);

    void Start(CancellationToken cancellationToken);

    void Stop();

    public string Name { get; }

    public Metrics Metrics { get; }
}