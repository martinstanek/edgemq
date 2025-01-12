using EdgeMq.Model;
using MudBlazor;

namespace EdgeMq.Dashboard.Pages;

public partial class MainView
{
    private QueueServer _server = QueueServer.Empty;
    private Timer? _timer;

    protected override Task OnInitializedAsync()
    {
        _timer = new Timer(async (s) => await OnTimerAsync() ,null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

        return Task.CompletedTask;
    }

    private async Task OnTimerAsync()
    {
        await ReloadAsync();

        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    private async Task ReloadAsync()
    {
        _server = await EdgeMqClient.GetQueuesAsync();

        foreach (var queue in _server.Queues)
        {
            EventingService.SignalMetrics(queue.Metrics);
        }
    }
}