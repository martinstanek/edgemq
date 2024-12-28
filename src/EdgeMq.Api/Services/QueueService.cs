using System;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service1;
using Microsoft.Extensions.Hosting;

namespace EdgeMq.Api.Services;

public sealed class QueueService : IHostedService
{
    private readonly IEdgeMq _queue;

    public QueueService(IEdgeMq queue)
    {
        _queue = queue;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _queue.Start(cancellationToken);

        Console.WriteLine($"Started: {_queue.Name}");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _queue.Stop();
        
        return Task.CompletedTask;
    }
}