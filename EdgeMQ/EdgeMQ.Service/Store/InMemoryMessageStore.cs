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
    private ulong _currentId;

    public Task InitAsync()
    {
        _currentCount = 0;
        _currentSize = 0;
        _currentId = 0;

        return Task.CompletedTask;
    }

    public Task AddMessagesAsync(IReadOnlyCollection<byte[]> messagePayloads)
    {
        foreach (var payload in messagePayloads)
        {
            var message = new Message
            {
                Id = GetNextId(),
                Payload = payload
            };

            _messages[message.Id] = message;

            Interlocked.Increment(ref _currentCount);
            Interlocked.Add(ref _currentSize, message.Payload.Length);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize)
    {
        throw new System.NotImplementedException();
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

    private ulong GetNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }

    public long MessageCount => _currentCount;

    public long MessageSizeBytes => _currentSize;

    public ulong CurrentCurrentId => _currentId;

    public long MaxMessageCount { get; }

    public long MaxMessageSizeBytes { get; }
}