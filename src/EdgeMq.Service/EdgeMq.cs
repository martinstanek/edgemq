using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using EdgeMq.Infra.Metrics;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using EdgeMq.Service.Model;
using EdgeMq.Service.Validation;
using EdgeMq.Service.Exceptions;
using EdgeMq.Service.Configuration;
using Ardalis.GuardClauses;

namespace EdgeMq.Service;

public sealed class EdgeMq : IEdgeMq
{
    private readonly List<Message> _peekedMessages = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly EventsPerInterval _messagesOut = new();
    private readonly EventsPerInterval _messagesIn = new();
    private readonly EdgeQueueConfiguration _configuration;
    private readonly InputBuffer _inputBuffer;
    private readonly IMessageStore _messageStore;
    private Guid _currentBatchId = Guid.Empty;
    private ulong _processedMessages;
    private bool _isStopped;

    public EdgeMq(InputBuffer inputBuffer, IMessageStore messageStore, EdgeQueueConfiguration configuration)
    {
        if (!Validations.IsConfigurationValid(configuration))
        {
            throw new EdgeConfigurationException("The configuration is invalid");
        }

        _inputBuffer = inputBuffer;
        _messageStore = messageStore;
        _configuration = configuration;
    }

    public Task<bool> EnqueueAsync(string payload, CancellationToken cancellationToken)
    {
        return EnqueueAsync(payload, ReadOnlyDictionary<string, string>.Empty, cancellationToken);
    }

    public Task<bool> EnqueueAsync(string payload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(payload);

        return _inputBuffer.TryAddAsync(payload, headers, cancellationToken);
    }

    public async Task DequeueAsync(uint batchSize, TimeSpan timeOut, Func<ImmutableArray<Message>, Task> process, CancellationToken cancellationToken)
    {
        Guard.Against.NegativeOrZero(batchSize);

        await _semaphore.WaitAsync(cancellationToken);

        var timeOutSource = new CancellationTokenSource(timeOut);
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeOutSource.Token, cancellationToken);

        try
        {
            await NonBlockingPeekAsync(batchSize);
            await Task.Run(() => process(_peekedMessages.ToImmutableArray()), linkedSource.Token);

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

    public async Task<ImmutableArray<Message>> DequeueAsync(uint batchSize, CancellationToken cancellationToken)
    {
        Guard.Against.NegativeOrZero(batchSize);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await NonBlockingPeekAsync(batchSize);
            await NonBlockingAcknowledgeAsync(_currentBatchId);

            return _peekedMessages.ToImmutableArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ImmutableArray<Message>> PeekAsync(uint batchSize, CancellationToken cancellationToken)
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
        Task.Factory.StartNew(
            () => ProcessBufferAsync(cancellationToken),
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public void Stop()
    {
        _isStopped = true;
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task<ImmutableArray<Message>> NonBlockingPeekAsync(uint batchSize)
    {
        _peekedMessages.Clear();
        _currentBatchId = Guid.NewGuid();

        var peekedMessages = await _messageStore.ReadMessagesAsync(batchSize);

        foreach (var message in peekedMessages)
        {
            _peekedMessages.Add(message with { BatchId = _currentBatchId });
        }

        return _peekedMessages.ToImmutableArray();
    }

    private async Task NonBlockingAcknowledgeAsync(Guid batchId)
    {
        if (_peekedMessages.Count == 0)
        {
            return;
        }

        if (!batchId.Equals(_currentBatchId))
        {
            throw new EdgeQueueAcknowledgeException("The batch id is obsolete");
        }

        if (!_peekedMessages.Any(p => p.BatchId.Equals(batchId)))
        {
            throw new EdgeQueueAcknowledgeException("The batch id is invalid");
        }

        var idsToDelete = _peekedMessages.Select(s => s.Id).ToImmutableArray();

        await _messageStore.DeleteMessagesAsync(idsToDelete);

        _messagesOut.AddEvents((uint) idsToDelete.Length);
        _processedMessages += (ulong) idsToDelete.Length;
    }

    private async Task ProcessBufferAsync(CancellationToken cancellationToken)
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
        await Task.Delay(_configuration.EdgeProcessingMessagesDelayMs, cancellationToken);

        if (_messageStore.IsFull)
        {
            return;
        }

        var messagesToStore = await ExtractIncomingMessagesAsync(cancellationToken);

        if (!messagesToStore.Any())
        {
            return;
        }

        await StoreIncomingMessagesAsync(messagesToStore);
    }

    private async Task<ImmutableArray<StoreMessage>> ExtractIncomingMessagesAsync(CancellationToken cancellationToken)
    {
        var incomingMessages = await _inputBuffer.ReadAllAsync(cancellationToken);

        return incomingMessages
            .Select(incomingMessage => new StoreMessage
            {
                Payload = incomingMessage.Payload,
                Headers = incomingMessage.Headers
            })
            .ToImmutableArray();
    }

    private async Task StoreIncomingMessagesAsync(ImmutableArray<StoreMessage> messagesToStore)
    {
        bool added;

        try
        {
            added = await _messageStore.AddMessagesAsync(messagesToStore);
        }
        catch
        {
            if (_configuration.ConstraintViolationMode == ConstraintViolationMode.ThrowException)
            {
                throw new EdgeQueueException("Failed to store incoming messages");
            }

            added = false;
        }

        if (added)
        {
            _messagesIn.AddEvents((uint) messagesToStore.Length);
        }
    }

    public string Name => _configuration.Name;

    public Metrics Metrics => new Metrics
    {
        MessageCount = _messageStore.MessageCount,
        MaxMessageCount = _messageStore.MaxMessageCount,
        MessageSizeBytes = _messageStore.MessageSizeBytes,
        MaxMessageSizeBytes = _messageStore.MaxMessageSizeBytes,
        BufferMessageCount = _inputBuffer.MessageMessageCount,
        MaxBufferMessageCount = _inputBuffer.MaxMessageCount,
        BufferMessageSizeBytes = _inputBuffer.MessageMessagesSize,
        MaxBufferMessageSizeBytes = _inputBuffer.MaxMessageSizeBytes,
        CurrentCurrentId = _messageStore.CurrentId,
        ProcessedMessages = _processedMessages,
        MessagesInPerSecond = _messagesIn.CurrentEventsPerSecond(),
        MessagesOutPerSecond = _messagesOut.CurrentEventsPerSecond()
    };
}