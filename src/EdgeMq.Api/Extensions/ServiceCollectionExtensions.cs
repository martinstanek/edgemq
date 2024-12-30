using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdgeMq.Api.Configuration;
using EdgeMq.Api.Handlers;
using EdgeMq.Api.Serialization;
using EdgeMq.Api.Services;
using EdgeMq.Service.Extensions;

namespace EdgeMq.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSerialization(this IServiceCollection services)
    {
        return services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });
    }

    public static IServiceCollection AddQueue(this IServiceCollection services, IConfiguration configuration)
    {
        var serverConfig = EdgeMqServerConfiguration.ReadFromEnvironment();

        return services
            .AddQueueManager(serverConfig.Queues, serverConfig.Path, serverConfig.Mode == QueueStoreMode.InMemory)
            .AddSingleton<IEdgeQueueHandler, EdgeQueueHandler>()
            .AddHostedService<QueueService>();
    }
}