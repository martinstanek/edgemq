using EdgeMq.Infra.Metrics;
using EdgeMq.Model;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EdgeMq.Dashboard.Components;

public partial class QueueThroughputChart
{
    [Parameter]
    public string QueueName { get; set; } = string.Empty;

    private sealed record ChartValue(string Label, double Value);

    private const int EdgeChartMaxValues = 15;

    private readonly LimitedSizeAddOnlyStack<ChartValue> _inValues = new(EdgeChartMaxValues);
    private readonly LimitedSizeAddOnlyStack<ChartValue> _outValues = new(EdgeChartMaxValues);
    private readonly ChartOptions _options = new();
    private int _chartIndex = -1;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        EventingService.OnMetrics += AddValues;
    }

    private async void AddValues(object? sender, QueueMetrics queueMetrics)
    {
        if (!queueMetrics.Name.Equals(QueueName))
        {
            return;
        }

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