using System.IO;
using EdgeMq.Service.Input;
using EdgeMq.Service.Store;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMq.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSystemEdgeMq(this IServiceCollection services, string name, string rootPath)
    {
        var storeConfig = new MessageStoreConfiguration()
        {
            Path = Path.Combine(rootPath, name)
        };

        var store = new FileSystemMessageStore(storeConfig);

        return AddEdgeMq(services, name, store);
    }

    public static IServiceCollection AddInMemoryEdgeMq(this IServiceCollection services, string name)
    {
        var storeConfig = new MessageStoreConfiguration();
        var store = new InMemoryMessageStore(storeConfig);

        return AddEdgeMq(services, name, store);
    }

    public static IServiceCollection AddEdgeMq(this IServiceCollection services, string name, IMessageStore store)
    {
        var queueConfig = new EdgeQueueConfiguration { Name = name };
        var bufferConfig = new InputBufferConfiguration();
        var buffer = new InputBuffer(bufferConfig);

        return services
            .AddSingleton(buffer)
            .AddSingleton(queueConfig)
            .AddSingleton<IMessageStore>(_ => store)
            .AddSingleton<IEdgeMq, EdgeMq>();
    }
}