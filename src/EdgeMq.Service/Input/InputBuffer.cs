using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EdgeMq.Service.Exceptions;

namespace EdgeMq.Service.Input;

public sealed class InputBuffer
{
    private readonly InputBufferConfiguration _configuration;
    private readonly Channel<BufferMessage> _inputChanel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private ulong _currentCount;
    private ulong _currentSize;

    public InputBuffer(InputBufferConfiguration configuration)
    {
        _inputChanel = Channel.CreateBounded<BufferMessage>((int) configuration.MaxMessageCount);
        _configuration = configuration;
    }

    public async Task AddAsync(string payload, CancellationToken cancellationToken)
    {
        if (!CheckConstraints(payload))
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);

        _currentSize += (uint) payload.Length;
        _currentCount++;

        var message = new BufferMessage
        {
            Payload = payload
        };

        try
        {
            await _inputChanel.Writer.WriteAsync(message, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyCollection<BufferMessage>> ReadAllAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        var messages = new List<BufferMessage>();

        try
        {
            while (_inputChanel.Reader.TryRead(out var message))
            {
                _currentSize -= (uint) message.Payload.Length;
                _currentCount--;

                messages.Add(message);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return messages;
    }

    private bool CheckConstraints(string payload)
    {
        var valid = !(payload.Length > _configuration.MaxMessageSizeBytes
                     || _currentSize + (uint) payload.Length > _configuration.MaxBufferSizeBytes
                     || _currentCount + 1 > _configuration.MaxMessageCount);

        if (!valid && _configuration.Mode == ConstraintViolationMode.ThrowException)
        {
            throw new EdgeQueueInputException("Input buffer constraints violated");
        }

        return valid;
    }

    public ulong MessageCount => _currentCount;

    public ulong MessageSizeBytes => _currentSize;

    public ulong MaxMessageCount => _configuration.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _configuration.MaxBufferSizeBytes;
}