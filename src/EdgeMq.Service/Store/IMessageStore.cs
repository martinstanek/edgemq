using System.Collections.Immutable;
using System.Threading.Tasks;
using EdgeMq.Service.Model;

namespace EdgeMq.Service.Store;

public interface IMessageStore
{
    Task InitAsync();

    Task<bool> AddMessagesAsync(ImmutableArray<StoreMessage> messages);

    Task<ImmutableArray<Message>> ReadMessagesAsync();

    Task<ImmutableArray<Message>> ReadMessagesAsync(uint batchSize);

    Task DeleteMessagesAsync(ImmutableArray<ulong> messageIds);

    public bool IsFull { get; }

    public ulong MessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong MaxMessageCount { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong CurrentId { get; }
}