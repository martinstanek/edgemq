using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using Ardalis.GuardClauses;

namespace EdgeMq.Service;

public sealed class QueueManager
{
    private readonly Dictionary<string, IEdgeMq> _queues = [];
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

        var store = _isInMemory
            ? (IMessageStore) new InMemoryMessageStore(queueConfiguration.StoreConfiguration)
            : new FileSystemMessageStore(queueConfiguration.StoreConfiguration);
        var buffer = new InputBuffer(queueConfiguration.BufferConfiguration);
        var queue = new EdgeMq(buffer, store, queueConfiguration);

        _queues[name] = queue;

        return this;
    }

    public IEdgeMq GetQueue(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        return _queues[name];
    }

    public IReadOnlyCollection<string> Queues => _queues.Select(s => s.Key).ToList();

    public bool IsInMemory => _isInMemory;
}