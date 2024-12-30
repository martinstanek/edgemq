using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.ConfigureSerialization();
builder.Services.AddQueue(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseApiEndpoints();
app.UseCors(p => { p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
app.MapOpenApi();
app.Run();

public partial class Program { }