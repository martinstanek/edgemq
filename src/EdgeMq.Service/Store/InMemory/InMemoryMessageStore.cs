using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using EdgeMq.Service.Configuration;
using EdgeMq.Service.Model;

namespace EdgeMq.Service.Store.InMemory;

public sealed class InMemoryMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<ulong, Message> _messages = new();
    private readonly MessageStoreConfiguration _configuration;
    private readonly Lock _lock = new();
    private ulong _currentCount;
    private ulong _currentSize;
    private ulong _currentId;

    public InMemoryMessageStore(MessageStoreConfiguration configuration)
    {
        _configuration = configuration;
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

    public Task<bool> AddMessagesAsync(ImmutableArray<StoreMessage> messages)
    {
        if (messages.Length == 0)
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            if (IsFull)
            {
                return Task.FromResult(false);
            }

            foreach (var m in messages)
            {
                var message = new Message
                {
                    Id = GetNextId(),
                    Payload = m.Payload,
                    Headers = m.Headers
                };

                _messages[message.Id] = message;
                _currentCount++;
                _currentSize += message.GetMessageSizeBytes();
            }
        }

        return Task.FromResult(true);
    }

    public Task<ImmutableArray<Message>> ReadMessagesAsync()
    {
        return ReadMessagesAsync(_configuration.DefaultBatchSize);
    }

    public Task<ImmutableArray<Message>> ReadMessagesAsync(uint batchSize)
    {
        if (batchSize == 0)
        {
            return Task.FromResult(ImmutableArray<Message>.Empty);
        }

        var result = _messages
            .OrderBy(m => m.Key)
            .Select(s => s.Value)
            .Take((int) batchSize)
            .ToImmutableArray();

        return Task.FromResult(result);
    }

    public Task DeleteMessagesAsync(ImmutableArray<ulong> messageIds)
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
                _currentSize -= deletedMessage.GetMessageSizeBytes();
            }
        }

        return Task.CompletedTask;
    }

    private ulong GetNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }

    public bool IsFull => _currentCount >= _configuration.MaxMessageCount || _currentSize >= _configuration.MaxMessageSizeBytes;

    public ulong MessageCount => _currentCount;

    public ulong MessageSizeBytes => _currentSize;

    public ulong CurrentId => _currentId;

    public ulong MaxMessageCount => _configuration.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _configuration.MaxMessageSizeBytes;
}