using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureSerialization();

var app = builder.Build();

app.ConfigureEndpoints();
app.Run();



