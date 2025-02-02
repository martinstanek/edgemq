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
    private ulong _currentMessageCount;
    private ulong _currentMessagesSize;

    public InputBuffer(InputBufferConfiguration configuration)
    {
        _inputChanel = Channel.CreateBounded<BufferMessage>((int) configuration.MaxMessageCount);
        _configuration = configuration;
    }

    public async Task<bool> TryAddAsync(string payload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!CheckConstraints(payload))
            {
                return false;
            }

            _currentMessagesSize += (uint) payload.Length;
            _currentMessageCount++;

            var message = new BufferMessage
            {
                Payload = payload,
                Headers = headers
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
                _currentMessagesSize -= (uint) message.Payload.Length;
                _currentMessageCount--;

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
        var isFull = _currentMessagesSize + payloadBytesCount > _configuration.MaxMessageSizeBytes;
        var isTooBig = payloadBytesCount > (int) _configuration.MaxPayloadSizeBytes;
        var isTooMany = _currentMessageCount + 1 > _configuration.MaxMessageCount;
        var valid = !(isFull || isTooBig || isTooMany);

        if (!valid && _configuration.Mode == ConstraintViolationMode.ThrowException)
        {
            throw new EdgeQueueInputException("Input buffer constraints violated");
        }

        return valid;
    }

    public ulong MessageMessageCount => _currentMessageCount;

    public ulong MessageMessagesSize => _currentMessagesSize;

    public ulong MaxMessageCount => _configuration.MaxMessageCount;

    public ulong MaxMessageSizeBytes => _configuration.MaxPayloadSizeBytes;
}