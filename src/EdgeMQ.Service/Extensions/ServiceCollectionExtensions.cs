using System;
using Microsoft.Extensions.DependencyInjection;

namespace EdgeMQ.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEdgeMq(this IServiceCollection services)
    {
        Console.WriteLine("EdgeMq");

        return services;
    }
}