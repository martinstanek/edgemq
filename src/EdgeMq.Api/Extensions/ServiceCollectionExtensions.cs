using EdgeMq.Api.Serialization;
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
}