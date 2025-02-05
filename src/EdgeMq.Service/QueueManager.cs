using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, IEdgeMq> _queues = [];
    private readonly bool _isInMemory;

    public QueueManager(bool isInMemory)
    {
        _isInMemory = isInMemory;
    }

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

    public QueueManager AddQueue(string name, EdgeQueueConfiguration queueConfiguration)
    {
        Guard.Against.NullOrWhiteSpace(name);

        if (!Validations.IsQueueNameValid(name))
        {
            throw new EdgeQueueException($"The provided name {name} for the queue is invalid.");
        }

        if (_queues.ContainsKey(name))
        {
            return this;
        }

        var store = _isInMemory
            ? (IMessageStore) new InMemoryMessageStore(queueConfiguration.StoreConfiguration)
            : new FileSystemMessageStore(queueConfiguration.StoreConfiguration);
        var buffer = new InputBuffer(queueConfiguration.BufferConfiguration);
        var queue = new EdgeMq(buffer, store, queueConfiguration);

        _queues[name] = queue;

        return this;
    }

    private IEdgeMq GetQueue(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        return _queues[name];
    }

    public IEdgeMq this[string name] => GetQueue(name);

    public IReadOnlyCollection<string> Queues => _queues.Keys.ToList();

    public bool IsInMemory => _isInMemory;
}