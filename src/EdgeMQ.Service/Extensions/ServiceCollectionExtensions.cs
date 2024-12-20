using EdgeMQ.Service.Input;
using EdgeMQ.Service.Store;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMQ.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdgeMq(this IServiceCollection services)
    {
        var storeConfig = new MessageStoreConfiguration();
        var bufferConfig = new InputBufferConfiguration();
        var buffer = new InputBuffer(bufferConfig);
        var store = new InMemoryMessageStore(storeConfig);

        return services
            .AddSingleton(buffer)
            .AddSingleton<IMessageStore>(_ => store)
            .AddSingleton<IEdgeMq, EdgeMq>();
    }
}