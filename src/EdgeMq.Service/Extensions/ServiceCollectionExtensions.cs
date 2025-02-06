using System.Collections.Immutable;
using EdgeMq.Service.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMq.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueueManager(
        this IServiceCollection services,
        ImmutableArray<EdgeQueueConfiguration> queueConfigs,
        bool isInMemory)
    {
        var manager = new QueueManager(isInMemory);

        foreach (var queueConfig in queueConfigs)
        {
            manager.AddQueue(queueConfig.Name, queueConfig);
        }

        return services.AddSingleton(manager);
    }
}