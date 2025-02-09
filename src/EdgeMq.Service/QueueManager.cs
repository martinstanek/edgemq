using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Exceptions;
using EdgeMq.Service.Store.FileSystem;
using EdgeMq.Service.Store.InMemory;
using EdgeMq.Service.Validation;
using Ardalis.GuardClauses;

namespace EdgeMq.Service;

public sealed class QueueManager
{
    private readonly ConcurrentDictionary<string, IEdgeQueue> _queues = [];

    public void Start(CancellationToken cancellationToken)
    {
        foreach (var queue in _queues)
        {
            queue.Value.Start(cancellationToken);
        }
    }

    public void Stop()
    {
        foreach (var queue in _queues)
        {
            queue.Value.Stop();
        }
    }

    public QueueManager AddQueue(EdgeQueueConfiguration queueConfiguration)
    {
        Guard.Against.NullOrWhiteSpace(queueConfiguration.Name);

        if (!Validations.IsQueueNameValid(queueConfiguration.Name))
        {
            throw new EdgeQueueException($"The provided name {queueConfiguration.Name} for the queue is invalid.");
        }

        if (_queues.ContainsKey(queueConfiguration.Name))
        {
            return this;
        }

        var store = queueConfiguration.IsInMemory
            ? (IMessageStore) new InMemoryMessageStore(queueConfiguration.StoreConfiguration)
            : new FileSystemMessageStore(queueConfiguration.StoreConfiguration);
        var buffer = new InputBuffer(queueConfiguration.BufferConfiguration);
        var queue = new EdgeQueue(buffer, store, queueConfiguration);

        _queues[queueConfiguration.Name] = queue;

        return this;
    }

    private IEdgeQueue GetQueue(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        return _queues[name];
    }

    public IEdgeQueue this[string name] => GetQueue(name);

    public ImmutableArray<string> QueueNames => _queues.Keys.ToImmutableArray();

    public ImmutableArray<IEdgeQueue> Queues => _queues.Values.ToImmutableArray();
}