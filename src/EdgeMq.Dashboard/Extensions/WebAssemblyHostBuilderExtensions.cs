using EdgeMq.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace EdgeMq.Dashboard.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddEdgeMqClient(this WebAssemblyHostBuilder builder)
    {
        var uri = new Uri(builder.HostEnvironment.BaseAddress);
        var httpClient = new HttpClient { BaseAddress = uri };
        var client = new EdgeMqClient(httpClient);

        builder.Services.AddSingleton<IEdgeMqClient>(_ =>  client);

        return builder;
    }
}