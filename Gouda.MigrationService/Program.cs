using Gouda.Database;
using Gouda.MigrationService;
using Gouda.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));
builder.Services.AddHostedService<Worker>();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<GoudaDbContext>(connectionName: "gouda");

var host = builder.Build();
host.Run();
