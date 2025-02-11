using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Client;

namespace EdgeMq.Producer;

public static class Program
{
    private static ulong _count;

    public static async Task Main()
    {
        Console.WriteLine("EdgeMQ Producer");

        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:2323") };
        var config = new EdgeMqClientConfiguration { ApiKey = "123" };
        var edgeMqClient = new EdgeMqClient(httpClient, config);
        var source = new CancellationTokenSource();
        var token = source.Token;
        var random = new Random();

        source.CancelAfter(TimeSpan.FromMinutes(10));

        while (!token.IsCancellationRequested)
        {
            var payload = DateTime.Now.ToString("s");

            await edgeMqClient.EnqueueAsync("queue1", payload);

            await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 100)), token);

            await edgeMqClient.EnqueueAsync("queue2", payload);

            await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 100)), token);

            Console.WriteLine($"{++_count} - {payload}");
        }
    }
}