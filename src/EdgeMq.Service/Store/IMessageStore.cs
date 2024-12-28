using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeMq.Service.Store;

public interface IMessageStore
{
    Task InitAsync();

    Task AddMessagesAsync(IReadOnlyCollection<string> messagePayloads);

    Task<IReadOnlyCollection<Message>> ReadMessagesAsync();

    Task<IReadOnlyCollection<Message>> ReadMessagesAsync(uint batchSize);

    Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds);

    public ulong MessageCount { get; }

    public ulong MessageSizeBytes { get; }

    public ulong MaxMessageCount { get; }

    public ulong MaxMessageSizeBytes { get; }

    public ulong CurrentId { get; }
}