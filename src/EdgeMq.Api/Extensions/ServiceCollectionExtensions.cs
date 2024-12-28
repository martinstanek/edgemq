using System.Linq;
using EdgeMq.Api.Configuration;
using EdgeMq.Api.Handlers;
using EdgeMq.Api.Serialization;
using EdgeMq.Api.Services;
using EdgeMq.Service.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        var queue = serverConfig.Queues.First();

        switch (serverConfig.Mode)
        {
            case QueueStoreMode.FileSystem:
                services.AddFileSystemEdgeMq(queue, serverConfig.Path);
                break;
            case QueueStoreMode.InMemory:
                services.AddInMemoryEdgeMq(queue);
                break;
        }

        return services
            .AddSingleton(serverConfig)
            .AddSingleton<IEdgeQueueHandler, EdgeQueueHandler>()
            .AddHostedService<QueueService>();
    }
}