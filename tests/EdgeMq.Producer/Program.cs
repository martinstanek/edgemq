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

        var httpClient = new HttpClient { BaseAddress = new Uri("http://10.0.1.106:2323") };
        var edgeMqClient = new EdgeMqClient(httpClient);
        var source = new CancellationTokenSource();
        var token = source.Token;

        source.CancelAfter(TimeSpan.FromMinutes(10));

        while (!token.IsCancellationRequested)
        {
            var payload = DateTime.Now.ToString("s");

            await edgeMqClient.EnqueueAsync("test-queue", payload);

            Console.WriteLine($"{++_count} - {payload}");

            await Task.Delay(100, token);
        }
    }
}