using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
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

    public static IServiceCollection AddQueue(this IServiceCollection services, IConfiguration configuration)
    {
        var serverConfig = EdgeMqServerConfiguration.ReadFromEnvironment();
        var queueConfigs = new List<EdgeQueueConfiguration>();

        foreach (var queue in serverConfig.Queues)
        {
            var config = new EdgeQueueConfiguration
            {
                Name = queue,
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

            queueConfigs.Add(config);
        }

        return services
            .AddQueueManager(queueConfigs, serverConfig.Mode == QueueStoreMode.InMemory)
            .AddSingleton<IEdgeQueueHandler, EdgeQueueHandler>()
            .AddHostedService<QueueService>();
    }
}