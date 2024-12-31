using EdgeMq.Model;
using MudBlazor;

namespace EdgeMq.Dashboard.Pages;

public partial class MainView
{
    private IReadOnlyCollection<Queue> _queues = [];
    private Timer? _timer;

    protected override async Task OnInitializedAsync()
    {
        _queues = await EdgeMqClient.GetQueuesAsync();
        _timer = new Timer(async (s) => await OnTimerAsync() ,null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
    }

    private async Task OnTimerAsync()
    {
        _queues = await EdgeMqClient.GetQueuesAsync();
        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }
}