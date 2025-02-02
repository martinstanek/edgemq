using System.Collections.Generic;
using System.Threading.Tasks;
using EdgeMq.Service.Model;

namespace EdgeMq.Service.Store;

public interface IMessageStore
{
    Task InitAsync();

    Task<bool> AddMessagesAsync(IReadOnlyCollection<StoreMessage> messages);

    Task<IReadOnlyCollection<Message>> ReadMessagesAsync();

    Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize);

    Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds);

    public bool IsFull { get; }

    public ulong MessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong MaxMessageCount { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong CurrentId { get; }
}