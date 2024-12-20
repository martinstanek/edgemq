﻿using Microsoft.Extensions.Hosting;
using EdgeMQ.Extensions;
using EdgeMQ.Service.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddEdgeMq("test-queue");
builder.Build().Run();