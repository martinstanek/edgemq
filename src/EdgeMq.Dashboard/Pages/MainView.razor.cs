using EdgeMq.Model;
using MudBlazor;

namespace EdgeMq.Dashboard.Pages;

public partial class MainView
{
    private IReadOnlyCollection<Queue> forecasts = [];

    protected override async Task OnInitializedAsync()
    {
        forecasts = await EdgeMqClient.GetQueuesAsync();
    }

    private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.
    public ChartOptions Options = new ChartOptions();

    public List<ChartSeries> Series = new List<ChartSeries>()
    {
        new ChartSeries() { Name = "Fossil", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        new ChartSeries() { Name = "Renewable", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
    };
    public string[] XAxisLabels = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };
}