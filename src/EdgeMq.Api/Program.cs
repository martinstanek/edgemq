using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddCors();
builder.Services.AddQueue();
builder.Services.AddOpenApi();
builder.Services.ConfigureSerialization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseApiEndpoints();
app.UseCors(p => { p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
app.MapOpenApi();
app.Run();

public partial class Program { }

// TODO dynamic readout/processing delays