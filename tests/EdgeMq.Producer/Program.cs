using System;
using System.Net.Http;
using System.Threading;
using EdgeMq.Client;

namespace EdgeMq.Producer;

public static class Program
{
    public static void Main()
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

            edgeMqClient.EnqueueAsync("test-queue", payload).GetAwaiter().GetResult();

            Console.WriteLine(payload);

            Thread.Sleep(100);
        }
    }
}