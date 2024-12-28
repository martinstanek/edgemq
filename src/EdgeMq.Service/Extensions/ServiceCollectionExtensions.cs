using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMq.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdgeMq(this IServiceCollection services, string name)
    {
        var queueConfig = new EdgeQueueConfiguration { Name = name };
        var storeConfig = new MessageStoreConfiguration();
        var bufferConfig = new InputBufferConfiguration();
        var buffer = new InputBuffer(bufferConfig);
        var store = new InMemoryMessageStore(storeConfig);

        return services
            .AddSingleton(buffer)
            .AddSingleton(queueConfig)
            .AddSingleton<IMessageStore>(_ => store)
            .AddSingleton<IEdgeMq, EdgeMq>();
    }
}