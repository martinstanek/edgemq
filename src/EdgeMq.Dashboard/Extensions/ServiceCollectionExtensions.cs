using EdgeMq.Client;

namespace EdgeMq.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdgeMqClient(this IServiceCollection services)
    {
        var uri = new Uri("http://10.0.1.106:2323" /*builder.HostEnvironment.BaseAddress*/);
        var httpClient = new HttpClient { BaseAddress = uri };
        var client = new EdgeMqClient(httpClient);

        return services.AddSingleton<IEdgeMqClient>(_ =>  client);
    }
}