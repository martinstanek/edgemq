using System.IO;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using EdgeMq.Api.Configuration;
using EdgeMq.Api.Handlers;
using EdgeMq.Api.Services;
using EdgeMq.Api.Serialization;
using EdgeMq.Service.Extensions;
using EdgeMq.Service.Configuration;

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

    public static IServiceCollection AddQueue(this IServiceCollection services)
    {
        var serverConfig = EdgeMqServerConfiguration.ReadFromEnvironment();
        var queueConfigs = serverConfig.Queues.Select(queue => FromConfig(queue, serverConfig)).ToImmutableArray();

        return services
            .AddSingleton(serverConfig)
            .AddQueueManager(queueConfigs)
            .AddSingleton<IEdgeQueueHandler, EdgeQueueHandler>()
            .AddHostedService<QueueService>();
    }

    private static EdgeQueueConfiguration FromConfig(string queue, EdgeMqServerConfiguration serverConfig)
    {
        return new EdgeQueueConfiguration
        {
            Name = queue,
            IsInMemory = serverConfig.StoreMode == QueueStoreMode.InMemory,
            BufferConfiguration = new InputBufferConfiguration
            {
                MaxMessageCount = serverConfig.MaxBufferMessageCount,
                MaxMessageSizeBytes = serverConfig.MaxBufferMessageSizeBytes,
                MaxPayloadSizeBytes = serverConfig.MaxPayloadSizeBytes
            },
            StoreConfiguration = new MessageStoreConfiguration
            {
                Path = Path.Combine(serverConfig.Path, queue),
                MaxMessageCount = serverConfig.MaxMessageCount,
                MaxMessageSizeBytes = serverConfig.MaxMessageSizeBytes,
                DefaultBatchSize = serverConfig.DefaultBatchSize
            },
            ConstraintViolationMode = ConstraintViolationMode.Ignore
        };
    }
}