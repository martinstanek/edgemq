using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.ConfigureSerialization();
builder.Services.AddQueue(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
//app.ConfigureRoot();
app.ConfigureEndpoints();
//app.ConfigureStaticFiles();
app.MapOpenApi();
app.UseCors(p => { p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
app.Run();

public partial class Program { }