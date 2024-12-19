using System;
using System.Linq;
using EdgeMq.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EdgeMq.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureEndpoints(this WebApplication webApplication)
    {
        var sampleTodos = new Todo[]
        {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };

        var todosApi = webApplication.MapGroup("/todos");

        todosApi.MapGet("/", () => sampleTodos);
        todosApi.MapGet("/{id}", (int id) =>
            sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());

        return webApplication;
    }
}