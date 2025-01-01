using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using EdgeMq.Service.Configuration;

namespace EdgeMq.Service.Store;

public sealed class FileSystemMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<ulong, FileInfo> _messages = [];
    private readonly MessageStoreConfiguration _configuration;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private ulong _currentCount;
    private ulong _currentSize;
    private ulong _currentId;

    public FileSystemMessageStore(MessageStoreConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InitAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!Directory.Exists(_configuration.Path))
            {
                Directory.CreateDirectory(_configuration.Path);
            }

            var files = Directory.GetFiles(_configuration.Path);

            foreach (var file in files)
            {
                if (!ulong.TryParse(file, out var id))
                {
                    continue;
                }

                var info = new FileInfo(file);

                _messages.TryAdd(id, info);
                _currentSize += (ulong) info.Length;
                _currentCount++;
            }

            _currentId = !_messages.IsEmpty
                ? _messages.Max(m => m.Key)
                : 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddMessagesAsync(IReadOnlyCollection<string> messagePayloads)
    {
        if (messagePayloads.Count == 0)
        {
            return false;
        }

        await _semaphore.WaitAsync();

        try
        {
            foreach (var messagePayload in messagePayloads)
            {
                _currentId++;

                var path = Path.Combine(_configuration.Path, _currentId.ToString());

                await File.WriteAllTextAsync(path, messagePayload);

                var info = new FileInfo(path);

                _messages.TryAdd(_currentId, info);
                _currentSize += (ulong) info.Length;
                _currentCount++;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    public Task<IReadOnlyCollection<Message>> ReadMessagesAsync()
    {
        return ReadMessagesAsync(_configuration.DefaultBatchSize);
    }

    public async Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize)
    {
        if (batchSize == 0)
        {
            return Array.Empty<Message>();
        }

        var result = new List<Message>();
        var files = _messages.Take((int) batchSize);

        await _semaphore.WaitAsync();

        try
        {
            foreach (var file in files)
            {
                var message = new Message
                {
                    Id = file.Key,
                    BatchId = Guid.Empty,
                    Payload = await File.ReadAllTextAsync(file.Value.FullName)
                };

                result.Add(message);
            }

            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds)
    {
        if (messageIds.Count == 0)
        {
            return;
        }

        await _semaphore.WaitAsync();

        try
        {
            foreach (var messageId in messageIds)
            {
                if (!_messages.TryGetValue(messageId, out var info))
                {
                    continue;
                }

                File.Delete(info.FullName);

                _messages.Remove(messageId, out _);
                _currentCount--;
                _currentSize -= (ulong) info.Length;
            }

            _currentId = !_messages.IsEmpty
                ? _messages.Max(m => m.Key)
                : 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool IsFull => false; //TODO implement

    public ulong MessageCount => _currentCount;

    public ulong MessageSizeBytes => _currentSize;

    public ulong CurrentId => _currentId;

    public ulong MaxMessageCount => _configuration.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _configuration.MaxMessageSizeBytes;
}