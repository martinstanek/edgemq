using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EdgeMQ.Service.Input;

public sealed class InputBuffer
{
    private readonly InputBufferConfiguration _configuration;
    private readonly Channel<BufferMessage> _inputChanel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long _currentCount;
    private long _currentSize;

    public InputBuffer(InputBufferConfiguration configuration)
    {
        _inputChanel = Channel.CreateBounded<BufferMessage>((int) configuration.MaxMessageCount);
        _configuration = configuration;
    }

    public async Task AddAsync(byte[] payload, CancellationToken cancellationToken)
    {
        if (!CheckConstraints(ref payload))
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);

        Interlocked.Add(ref _currentSize, payload.Length);
        Interlocked.Increment(ref _currentCount);

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
                Interlocked.Add(ref _currentSize, -1 * message.Payload.Length);
                Interlocked.Decrement(ref _currentCount);

                messages.Add(message);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return messages;
    }

    private bool CheckConstraints(ref byte[] payload)
    {
        var valid = !(payload.Length > _configuration.MaxMessageSizeBytes
                     || _currentSize + payload.Length > (long) _configuration.MaxBufferSizeBytes
                     || _currentCount + 1 > _configuration.MaxMessageCount);

        if (!valid && _configuration.Mode == ConstraintViolationMode.ThrowException)
        {
            throw new EdgeQueueInputException();
        }

        return valid;
    }

    public long MessagesCount => _currentCount;

    public long PayloadSize => _currentSize;
}