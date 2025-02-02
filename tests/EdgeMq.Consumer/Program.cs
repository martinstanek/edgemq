using EdgeMq.Client;

namespace EdgeMq.Consumer;

public static class Program
{
    private static ulong _count;

    public static async Task Main()
    {
        Console.WriteLine("EdgeMQ Consumer");

        var r = new Random();
        var httpClient = new HttpClient { BaseAddress = new Uri("http://10.0.1.106:2323") };
        var edgeMqClient = new EdgeMqClient(httpClient);
        var source = new CancellationTokenSource();
        var token = source.Token;

        source.CancelAfter(TimeSpan.FromMinutes(10));

        while (!token.IsCancellationRequested)
        {
            var batch = r.Next(1, 20);

            await edgeMqClient.DequeueAsync("test-queue", batch, TimeSpan.FromSeconds(1), messages =>
            {
                foreach (var message in messages)
                {
                    Console.WriteLine($"{++_count} - {message.Payload}");
                }

                return Task.CompletedTask;

            }, CancellationToken.None);

            await Task.Delay(100, token);
        }
    }
}