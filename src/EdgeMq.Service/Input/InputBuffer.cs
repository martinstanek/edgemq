using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EdgeMq.Service.Configuration;
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

    public async Task<bool> AddAsync(string payload, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!CheckConstraints(payload))
            {
                return false;
            }

            _currentSize += (uint) payload.Length;
            _currentCount++;

            var message = new BufferMessage
            {
                Payload = payload
            };

            await _inputChanel.Writer.WriteAsync(message, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
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
        var payloadBytesCount = (uint) Encoding.UTF8.GetByteCount(payload);
        var isFull = _currentSize + payloadBytesCount > _configuration.MaxMessageSizeBytes;
        var isTooBig = payloadBytesCount > (int) _configuration.MaxPayloadSizeBytes;
        var isTooMany = _currentCount + 1 > _configuration.MaxMessageCount;
        var valid = !(isFull || isTooBig || isTooMany);

        if (!valid && _configuration.Mode == ConstraintViolationMode.ThrowException)
        {
            throw new EdgeQueueInputException("Input buffer constraints violated");
        }

        return valid;
    }

    public ulong MessageCount => _currentCount;

    public ulong MessageSizeBytes => _currentSize;

    public ulong MaxMessageCount => _configuration.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _configuration.MaxPayloadSizeBytes;
}