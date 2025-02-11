using EdgeMq.Model;

namespace EdgeMq.Dashboard.Eventing;

public sealed class QueueMetricsEventingService : IQueueMetricsEventingService
{
    public void SignalMetrics(QueueMetrics queueMetrics)
    {
        OnMetrics.Invoke(this, queueMetrics);
    }

    public event EventHandler<QueueMetrics> OnMetrics = (_, _) => { };
}