using EdgeMq.Model;

namespace EdgeMq.Dashboard.Eventing;

public interface IQueueMetricsEventingService
{
    void SignalMetrics(QueueMetrics queueMetrics);

    event EventHandler<QueueMetrics> OnMetrics;
}