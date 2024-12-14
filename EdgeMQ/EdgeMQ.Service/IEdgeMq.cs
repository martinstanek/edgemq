using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeMQ.Service;

public interface IEdgeMq
{
    Task QueueAsync(byte[] payload);

    Task<IReadOnlyCollection<Message>> DeQueueAsync();
}