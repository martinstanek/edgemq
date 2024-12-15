using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeMQ.Service.Store;

public interface IMessageStore
{
    Task InitAsync();

    Task AddMessagesAsync(IReadOnlyCollection<Message> messages);

    Task DeleteMessagesAsync(IReadOnlyCollection<ulong> messageIds);

    public long MessageCount { get; }

    public long MessageSizeBytes { get; }

    public long MaxMessageCount { get; }

    public long MaxMessageSizeBytes { get; }
}