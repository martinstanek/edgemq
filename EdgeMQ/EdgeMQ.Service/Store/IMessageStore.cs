using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeMQ.Service.Store;

public interface IMessageStore
{
    Task InitAsync();

    Task AddMessagesAsync(IReadOnlyCollection<byte[]> messagePayloads);

    Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize);

    Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds);

    public long MessageCount { get; }

    public long MessageSizeBytes { get; }

    public long MaxMessageCount { get; }

    public long MaxMessageSizeBytes { get; }

    public ulong CurrentCurrentId { get; }
}