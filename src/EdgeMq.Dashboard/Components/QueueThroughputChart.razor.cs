using EdgeMq.Infra.Metrics;
using EdgeMq.Model;
using MudBlazor;

namespace EdgeMq.Dashboard.Components;

public partial class QueueThroughputChart
{
    private sealed record ChartValue(string Label, double Value);

    private const int MaxValues = 15;

    private readonly LimitedSizeAddOnlyStack<ChartValue> _inValues = new(MaxValues);
    private readonly LimitedSizeAddOnlyStack<ChartValue> _outValues = new(MaxValues);
    private readonly ChartOptions _options = new();
    private int _chartIndex = -1;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        EventingService.OnMetrics += AddValues;
    }

    private async void AddValues(object? sender, QueueMetrics queueMetrics)
    {
        var dateTime = $"{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}";

        _inValues.Push(new ChartValue(dateTime, queueMetrics.MessagesInPerSecond));
        _outValues.Push(new ChartValue(dateTime, queueMetrics.MessagesOutPerSecond));

        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    private List<ChartSeries> Series =>
    [
        new() { Name = "In/s", Data = _inValues.Items.Reverse().Select(s => s.Value).ToArray() },
        new() { Name = "Out/s", Data = _outValues.Items.Reverse().Select(s => s.Value).ToArray() }
    ];

    private string[] XAxisLabels => _inValues.Items.Reverse().Select(s => s.Label).ToArray();
}