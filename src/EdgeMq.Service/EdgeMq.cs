using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using EdgeMq.Infra.Metrics;
using EdgeMq.Service.Exceptions;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;

namespace EdgeMq.Service;

public sealed class EdgeMq : IEdgeMq
{
    private readonly ConcurrentBag<Message> _peekedMessages = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly EventsPerInterval _messagesOut = new();
    private readonly EventsPerInterval _messagesIn = new();
    private readonly EdgeQueueConfiguration _configuration;
    private readonly InputBuffer _inputBuffer;
    private readonly IMessageStore _messageStore;
    private Guid _currentBatchId = Guid.Empty;
    private bool _isStopped;

    public EdgeMq(InputBuffer inputBuffer, IMessageStore messageStore, EdgeQueueConfiguration configuration)
    {
        _inputBuffer = inputBuffer;
        _messageStore = messageStore;
        _configuration = configuration;
    }

    public Task QueueAsync(string payload, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(payload);

        return _inputBuffer.AddAsync(payload, cancellationToken);
    }

    public async Task DeQueueAsync(uint batchSize, TimeSpan timeOut, Func<IReadOnlyCollection<Message>, Task> process, CancellationToken cancellationToken)
    {
        Guard.Against.NegativeOrZero(batchSize);

        await _semaphore.WaitAsync(cancellationToken);

        var timeOutSource = new CancellationTokenSource(timeOut);
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeOutSource.Token, cancellationToken);

        try
        {
            await NonBlockingPeekAsync(batchSize);
            await Task.Run(() => process(_peekedMessages), linkedSource.Token);

            if (_peekedMessages.Count != 0)
            {
                await NonBlockingAcknowledgeAsync(_currentBatchId);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyCollection<Message>> DeQueueAsync(uint batchSize, CancellationToken cancellationToken)
    {
        Guard.Against.NegativeOrZero(batchSize);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await NonBlockingPeekAsync(batchSize);
            await NonBlockingAcknowledgeAsync(_currentBatchId);

            return _peekedMessages;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyCollection<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken)
    {
        Guard.Against.NegativeOrZero(batchSize);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return await NonBlockingPeekAsync(batchSize);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task AcknowledgeAsync(Guid batchId, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await NonBlockingAcknowledgeAsync(batchId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Start(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(() => ProcessQueueAsync(cancellationToken), TaskCreationOptions.LongRunning);
    }

    public void Stop()
    {
        _isStopped = true;
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task<IReadOnlyCollection<Message>> NonBlockingPeekAsync(uint batchSize)
    {
        _peekedMessages.Clear();
        _currentBatchId = Guid.NewGuid();

        var peekedMessages = await _messageStore.ReadMessagesAsync(batchSize);

        foreach (var message in peekedMessages)
        {
            _peekedMessages.Add(message with { BatchId = _currentBatchId });
        }

        return _peekedMessages;
    }

    private async Task NonBlockingAcknowledgeAsync(Guid batchId)
    {
        if (!batchId.Equals(_currentBatchId))
        {
            throw new EdgeQueueAcknowledgeException("The batch id is obsolete");
        }

        if (!_peekedMessages.Any(p => p.BatchId.Equals(batchId)))
        {
            throw new EdgeQueueAcknowledgeException("The batch id is invalid");
        }

        var idsToDelete = _peekedMessages.Select(s => s.Id).ToList();

        await _messageStore.DeleteMessagesAsync(idsToDelete);

        _messagesOut.AddEvents((uint) idsToDelete.Count);
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
                await Task.Delay(100, cancellationToken);
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

        _messagesIn.AddEvents((uint) messagePayloadsToStore.Count);
    }

    public string Name => _configuration.Name;

    public ulong MessageCount => _messageStore.MessageCount;

    public ulong MaxMessageCount => _messageStore.MaxMessageCount;

    public ulong MessageSizeBytes => _messageStore.MessageSizeBytes;

    public ulong MaxMessageSizeBytes => _messageStore.MaxMessageSizeBytes;

    public ulong BufferMessageCount => _inputBuffer.MessageCount;

    public ulong MaxBufferMessageCount => _inputBuffer.MaxMessageCount;

    public ulong BufferMessageSizeBytes => _inputBuffer.MessageSizeBytes;

    public ulong MaxBufferMessageSizeBytes => _inputBuffer.MaxMessageSizeBytes;

    public ulong CurrentCurrentId => _messageStore.CurrentId;

    public double MessagesInPerSecond => _messagesIn.CurrentEventsPerSecond();

    public double MessagesOutPerSecond => _messagesOut.CurrentEventsPerSecond();
}