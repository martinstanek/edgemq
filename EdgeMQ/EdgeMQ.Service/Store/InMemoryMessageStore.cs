using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeMQ.Service.Store;

public sealed class InMemoryMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<ulong, Message> _messages = new();
    private long _currentCount;
    private long _currentSize;
    private long _maxId;

    public Task InitAsync()
    {
        _currentCount = 0;
        _currentSize = 0;
        _maxId = 0;

        return Task.CompletedTask;
    }

    public Task AddMessagesAsync(IReadOnlyCollection<Message> messages)
    {
        foreach (var message in messages)
        {
            _messages[message.Id] = message;

            Interlocked.Increment(ref _currentCount);
            Interlocked.Add(ref _currentSize, message.Payload.Length);
        }

        return Task.CompletedTask;
    }

    public Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds)
    {
        foreach (var messageId in messageIds)
        {
            _messages.Remove(messageId, out var deletedMessage);

            if (deletedMessage is not null)
            {
                Interlocked.Decrement(ref _currentCount);
                Interlocked.Add(ref _currentSize, -1 * deletedMessage.Payload.Length);
            }
        }

        return Task.CompletedTask;
    }

    public long MessageCount => _currentCount;

    public long MessageSizeBytes => _currentSize;

    public long MaxMessageCount { get; }

    public long MaxMessageSizeBytes { get; }

    public long MaxId => _maxId;
}