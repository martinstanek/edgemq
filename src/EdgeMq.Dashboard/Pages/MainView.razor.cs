using EdgeMq.Model;

namespace EdgeMq.Dashboard.Pages;

public partial class MainView
{
    private QueueServer _server = QueueServer.Empty;
    private bool _autoRefresh;
    private bool _rendered;

    protected override Task OnInitializedAsync()
    {
        _ = new Timer(
            callback: async void (_) => await OnTimerAsync(),
            state: null,
            dueTime: TimeSpan.FromSeconds(1),
            period: TimeSpan.FromSeconds(2));

        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_rendered)
        {
            return;
        }

        await ReloadAndSignalAsync();

        _rendered = true;
    }

    private async Task OnTimerAsync()
    {
        if (!_autoRefresh)
        {
            return;
        }

        await ReloadAndSignalAsync();
    }

    private async Task ReloadAndSignalAsync()
    {
        await ReloadAsync();
        await InvokeAsync(StateHasChanged);
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