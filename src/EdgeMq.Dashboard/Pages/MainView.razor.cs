using EdgeMq.Model;

namespace EdgeMq.Dashboard.Pages;

public partial class MainView
{
    private IReadOnlyCollection<Queue> forecasts = [];

    protected override async Task OnInitializedAsync()
    {
        forecasts = await EdgeMqClient.GetQueuesAsync();
    }
}