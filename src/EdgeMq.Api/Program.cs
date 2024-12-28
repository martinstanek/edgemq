using EdgeMq.Api.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.ConfigureSerialization();
builder.Services.AddQueue(builder.Configuration);

var app = builder.Build();

app.ConfigureRoot();
app.ConfigureEndpoints();
app.Run();

public partial class Program { }