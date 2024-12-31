using EdgeMq.Dashboard.Eventing;

namespace EdgeMq.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventingServices(this IServiceCollection services)
    {
        return services.AddSingleton<IQueueMetricsEventingService, QueueMetricsEventingService>();
    }
}