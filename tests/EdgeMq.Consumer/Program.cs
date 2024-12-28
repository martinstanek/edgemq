using EdgeMq.Client;

namespace EdgeMq.Consumer;

public static class Program
{
    private static ulong _count;

    public static async Task Main()
    {
        Console.WriteLine("EdgeMQ Consumer");

        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:2323")
        };

        var edgeMqClient = new EdgeMqClient(httpClient);

        while (true)
        {
            await edgeMqClient.DequeueAsync("test-queue", 100, TimeSpan.FromSeconds(1), messages =>
            {
                foreach (var message in messages)
                {
                    Console.WriteLine($"{++_count} - {message.Payload}");
                }

                return Task.CompletedTask;

            }, CancellationToken.None);

            await Task.Delay(100);
        }
    }
}