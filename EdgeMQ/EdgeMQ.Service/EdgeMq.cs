using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EdgeMQ.Service.Input;
using EdgeMQ.Service.Store;

namespace EdgeMQ.Service;

public sealed class EdgeMq : IEdgeMq
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly InputBuffer _inputBuffer;
    private readonly IMessageStore _messageStore;

    private bool _isStopped;

    public EdgeMq(InputBuffer inputBuffer, IMessageStore messageStore)
    {
        _inputBuffer = inputBuffer;
        _messageStore = messageStore;
    }

    public Task QueueAsync(byte[] payload, CancellationToken cancellationToken)
    {
        return _inputBuffer.AddAsync(payload, cancellationToken);
    }

    public Task DeQueueAsync(uint batchSize, Func<Task, IReadOnlyCollection<Message>> process, TimeSpan timeOut, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AckAsync(string batchId)
    {
        throw new NotImplementedException();
    }

    public async Task DeQueueAsync(uint batchSize, Func<Task, IReadOnlyCollection<Message>> process, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {

        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Start(CancellationToken cancellationToken)
    {
        _ = ProcessQueueAsync(cancellationToken);
    }

    public void Stop()
    {
        _isStopped = true;
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        await _messageStore.InitAsync();

        while (!(cancellationToken.IsCancellationRequested || _isStopped))
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                await PersistIncomingMessagesAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    private async Task PersistIncomingMessagesAsync(CancellationToken cancellationToken)
    {
        var incomingMessages = await _inputBuffer.ReadAllAsync(cancellationToken);

        if (!incomingMessages.Any())
        {
            return;
        }

        var messagePayloadsToStore = incomingMessages
            .Select(incomingMessage => incomingMessage.Payload)
            .ToList();

        await _messageStore.AddMessagesAsync(messagePayloadsToStore);
    }
}