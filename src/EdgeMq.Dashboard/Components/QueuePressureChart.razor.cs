using EdgeMq.Infra.Metrics;
using EdgeMq.Model;
using MudBlazor;

namespace EdgeMq.Dashboard.Components;

public partial class QueuePressureChart
{
    private double _sizePressure = 0d;
    private double _sizeBufferPressure = 0;
    private double _countPressure = 0d;
    private double _countBufferPressure = 0d;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        EventingService.OnMetrics += OnMetrics;
    }

    private async void OnMetrics(object? sender, QueueMetrics queueMetrics)
    {
        _sizePressure = queueMetrics.MessagesSizePressure * 100;
        _sizeBufferPressure = queueMetrics.BufferMessagesSizePressure * 100;
        _countPressure = queueMetrics.MessageCountPressure * 100;
        _countBufferPressure = queueMetrics.BufferMessageCountPressure * 100;

        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }
}