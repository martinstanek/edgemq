﻿using EdgeMQ.Extensions;
using Microsoft.Extensions.Hosting;
using EdgeMQ.Service.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddEdgeMq();
builder.Build().Run();