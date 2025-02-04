using EdgeMq.Model;
using Microsoft.AspNetCore.Components;

namespace EdgeMq.Dashboard.Components;

public partial class QueuePressureChart
{
    [Parameter]
    public string QueueName { get; set; } = string.Empty;

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
        if (!queueMetrics.Name.Equals(QueueName))
        {
            return;
        }

        var adjustedCountPressures = GetAdjustedPressures(
            queueMetrics.MaxMessageCount,
            queueMetrics.MaxBufferMessageCount,
            queueMetrics.MessageCountPressure,
            queueMetrics.BufferMessageCountPressure);


        var adjustedSizePressures = GetAdjustedPressures(
            queueMetrics.MessagesSizeBytes,
            queueMetrics.MaxBufferMessagesSizeBytes,
            queueMetrics.MessagesSizePressure,
            queueMetrics.BufferMessagesSizePressure);

        _sizePressure = adjustedSizePressures.AdjustedPressure;
        _sizeBufferPressure = adjustedSizePressures.AdjustedBufferPressure;
        _countPressure = adjustedCountPressures.AdjustedPressure;
        _countBufferPressure = adjustedCountPressures.AdjustedBufferPressure;

        await InvokeAsync(StateHasChanged);
    }

    private static (double AdjustedPressure, double AdjustedBufferPressure) GetAdjustedPressures(
        ulong maxCount,
        ulong maxBufferCount,
        double pressure,
        double bufferPressure)
    {
        var percent = 100 / (double) (maxCount + maxBufferCount);
        var countRatio = maxCount * percent / 100;
        var adjustedPressure = pressure * countRatio * 100;
        var adjustedBufferPressure = ((bufferPressure * (1 - countRatio)) + countRatio) * 100;

        return (adjustedPressure, adjustedBufferPressure);
    }
}