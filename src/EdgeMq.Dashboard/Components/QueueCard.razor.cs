using EdgeMq.Model;
using Microsoft.AspNetCore.Components;

namespace EdgeMq.Dashboard.Components;

public partial class QueueCard
{
    [Parameter]
    public Queue Queue { get; set; } = Queue.Empty;

    private static string GetPressureString(ulong max, ulong value, double pressure, string unit = "")
    {
        return $"{pressure * 100:F}% ({value}{unit} / {max}{unit})";
    }

    private string CurrentSize =>
        $"{Queue.Metrics.MessagesSizeBytes}/B";

    private string MessagesInPerSecond =>
        $"{Queue.Metrics.MessagesInPerSecond:F2}/s";

    private string MessagesOutPerSecond =>
        $"{Queue.Metrics.MessagesOutPerSecond:F2}/s";

    private string BufferSizePressure =>
        GetPressureString(
            Queue.Metrics.MaxBufferMessagesSizeBytes,
            Queue.Metrics.BufferMessagesSizeBytes,
            Queue.Metrics.BufferMessagesSizePressure,
            unit: "B");

    private string BufferCountPressure =>
        GetPressureString(
            Queue.Metrics.MaxBufferMessageCount,
            Queue.Metrics.BufferMessageCount,
            Queue.Metrics.BufferMessageCountPressure);

    private string SizePressure =>
        GetPressureString(
            Queue.Metrics.MaxMessagesSizeBytes,
            Queue.Metrics.MessagesSizeBytes,
            Queue.Metrics.MessagesSizePressure,
            unit: "B");

    private string CountPressure =>
        GetPressureString(
            Queue.Metrics.MaxMessageCount,
            Queue.Metrics.MessageCount,
            Queue.Metrics.MessageCountPressure);
}