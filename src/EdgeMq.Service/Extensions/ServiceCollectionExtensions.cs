using System.Collections.Immutable;
using EdgeMq.Service.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMq.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueueManager(
        this IServiceCollection services,
        ImmutableArray<EdgeQueueConfiguration> queueConfigs)
    {
        var manager = new QueueManager();

        foreach (var queueConfig in queueConfigs)
        {
            manager.AddQueue(queueConfig);
        }

        return services.AddSingleton(manager);
    }
}