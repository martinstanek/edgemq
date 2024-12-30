using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;

namespace EdgeMq.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueueManager(
        this IServiceCollection services,
        IReadOnlyCollection<string> queues,
        string rootPath,
        bool isInMemory)
    {
        var manager = new QueueManager(isInMemory);

        foreach (var queue in queues)
        {
            var bufferConfig = new InputBufferConfiguration();
            var storeConfig = new MessageStoreConfiguration
            {
                Path = Path.Combine(rootPath, queue)
            };
            var queueConfig = new EdgeQueueConfiguration
            {
                Name = queue,
                BufferConfiguration = bufferConfig,
                StoreConfiguration = storeConfig
            };

            manager.AddQueue(queue, queueConfig);
        }

        return services.AddSingleton(manager);
    }
}