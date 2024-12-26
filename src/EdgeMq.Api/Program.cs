using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureSerialization();
builder.Services.AddQueue();

var app = builder.Build();

app.ConfigureEndpoints();
app.Run();

public partial class Program { }