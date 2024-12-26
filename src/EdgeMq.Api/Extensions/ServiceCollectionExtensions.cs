using EdgeMq.Api.Handlers;
using EdgeMq.Api.Serialization;
using EdgeMq.Api.Services;
using EdgeMQ.Service.Extensions;
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

    public static IServiceCollection AddQueue(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEdgeQueueHandler, EdgeQueueHandler>()
            .AddEdgeMq("test-queue")
            .AddHostedService<QueueService>();
    }
}