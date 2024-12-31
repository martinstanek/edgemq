using EdgeMq.Infra.Metrics;
using MudBlazor;

namespace EdgeMq.Dashboard.Components;

public partial class QueueThroughputChart
{
    private sealed record ChartValue(string Label, double Value);

    private const int MaxValues = 30;

    private readonly LimitedSizeAddOnlyStack<ChartValue> _inValues = new(MaxValues);
    private readonly LimitedSizeAddOnlyStack<ChartValue> _outValues = new(MaxValues);
    private readonly ChartOptions _options = new();
    private int _chartIndex = -1;

    public void AddValues(double inPerSecond, double outPerSecond)
    {
        var dateTime = $"{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";

        _inValues.Push(new ChartValue(dateTime, inPerSecond));
        _outValues.Push(new ChartValue(dateTime, outPerSecond));
    }

    private readonly List<ChartSeries> _series = new()
    {
        new ChartSeries { Name = "In/s", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        new ChartSeries { Name = "Out/s", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
    };

    private string[] _xAxisLabels = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

}