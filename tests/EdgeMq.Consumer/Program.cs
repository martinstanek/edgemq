using EdgeMq.Client;

namespace EdgeMq.Consumer;

public static class Program
{
    private static ulong _count;

    public static async Task Main()
    {
        Console.WriteLine("EdgeMQ Consumer");

        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
        var config = new EdgeMqClientConfiguration { ApiKey = "123" };
        var edgeMqClient = new EdgeMqClient(httpClient, config);
        var source = new CancellationTokenSource();
        var token = source.Token;
        var random = new Random();

        source.CancelAfter(TimeSpan.FromMinutes(10));

        while (!token.IsCancellationRequested)
        {
            var batch1 = random.Next(1, 200);
            var batch2 = random.Next(1, 200);

            await edgeMqClient.DequeueAsync("queue1", batch1, TimeSpan.FromSeconds(1), messages =>
            {
                foreach (var message in messages)
                {
                    Console.WriteLine($"{++_count} - {message.Payload}");
                }

                return Task.CompletedTask;

            }, CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 5)), token);

            await edgeMqClient.DequeueAsync("queue2", batch2, TimeSpan.FromSeconds(1), messages =>
            {
                foreach (var message in messages)
                {
                    Console.WriteLine($"{++_count} - {message.Payload}");
                }

                return Task.CompletedTask;

            }, CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 5)), token);
        }
    }
}