using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeMQ.Service.Store;

public sealed class InMemoryMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<ulong, Message> _messages = new();
    private readonly MessageStoreConfiguration _config;
    private readonly Lock _lock = new();
    private ulong _currentCount;
    private ulong _currentSize;
    private ulong _currentId;

    public InMemoryMessageStore(MessageStoreConfiguration config)
    {
        _config = config;
    }

    public Task InitAsync()
    {
        lock (_lock)
        {
            _currentCount = 0;
            _currentSize = 0;
            _currentId = 0;
        }

        return Task.CompletedTask;
    }

    public Task AddMessagesAsync(IReadOnlyCollection<string> messagePayloads)
    {
        lock (_lock)
        {
            foreach (var payload in messagePayloads)
            {
                var message = new Message
                {
                    Id = GetNextId(),
                    Payload = payload
                };

                _messages[message.Id] = message;
                _currentCount++;
                _currentSize += (ulong)message.Payload.Length;
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Message>> ReadMessagesAsync()
    {
        return ReadMessagesAsync(_config.DefaultBatchSize);
    }

    public Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize)
    {
        var result = _messages
            .OrderBy(m => m.Key)
            .Select(s => s.Value)
            .Take((int)batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<Message>>(result);
    }

    public Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds)
    {
        lock (_lock)
        {
            foreach (var messageId in messageIds)
            {
                _messages.Remove(messageId, out var deletedMessage);

                if (deletedMessage is null)
                {
                    continue;
                }

                _currentCount--;
                _currentSize -= (uint)deletedMessage.Payload.Length;
            }
        }

        return Task.CompletedTask;
    }

    private ulong GetNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }

    public ulong MessageCount => _currentCount;

    public ulong MessageSizeBytes => _currentSize;

    public ulong CurrentCurrentId => _currentId;

    public ulong MaxMessageCount => _config.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _config.MaxMessageSizeBytes;
}