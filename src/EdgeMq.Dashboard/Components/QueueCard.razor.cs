using EdgeMq.Model;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EdgeMq.Dashboard.Components;

public partial class QueueCard
{
    [Parameter] public Queue queue { get; set; } = Queue.Empty;

    private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

    public string GetBufferSizePressure(QueueMetrics metrics) =>
        GetPressureString(
            metrics.BufferMessagesSizeBytes,
            metrics.MaxBufferMessagesSizeBytes,
            metrics.BufferMessagesSizePressure,
            unit: "B");

    public string GetBufferCountPressure(QueueMetrics metrics) =>
        GetPressureString(
            metrics.BufferMessageCount,
            metrics.MaxBufferMessageCount,
            metrics.BufferMessageCountPressure);

    public string GetSizePressure(QueueMetrics metrics) =>
        GetPressureString(
            metrics.MessagesSizeBytes,
            metrics.MaxMessagesSizeBytes,
            metrics.MessagesSizePressure,
            unit: "B");

    public string GetCountPressure(QueueMetrics metrics) =>
        GetPressureString(
            metrics.MessageCount,
            metrics.MaxMessageCount,
            metrics.MessageCountPressure);

    private string GetPressureString(ulong max, ulong value, double pressure, string unit = "")
    {
        return $"{pressure * 100:F}% ({value}{unit} / {max}{unit})";
    }


    public ChartOptions Options = new ChartOptions();

    public List<ChartSeries> Series = new List<ChartSeries>()
    {
        new ChartSeries() { Name = "Fossil", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        new ChartSeries() { Name = "Renewable", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
    };

    public string[] XAxisLabels = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

}