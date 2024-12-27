using System;
using System.Net.Http;
using System.Threading.Tasks;
using EdgeMq.Client;

namespace EdgeMq.Producer;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("EdgeMQ Producer");

        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:2323")
        };

        var edgeMqClient = new EdgeMqClient(httpClient);

        while (true)
        {
            var payload = DateTime.Now.ToString("s");

            await edgeMqClient.EnqueueAsync("test-queue", payload);

            Console.WriteLine(payload);

            await Task.Delay(100);
        }
    }
}