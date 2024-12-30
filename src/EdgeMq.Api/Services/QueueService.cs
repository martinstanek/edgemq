using System;
using System.Threading;
using System.Threading.Tasks;
using EdgeMq.Service;
using Microsoft.Extensions.Hosting;

namespace EdgeMq.Api.Services;

public sealed class QueueService : IHostedService
{
    private readonly QueueManager _queueManager;

    public QueueService(QueueManager queueManager)
    {
        _queueManager = queueManager;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _queueManager.Start(cancellationToken);

        Console.WriteLine($"Started: {string.Join(',', _queueManager.Queues)}");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _queueManager.Stop();
        
        return Task.CompletedTask;
    }
}