using Deke.Infrastructure;
using Deke.Worker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

// Database
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? throw new InvalidOperationException("Connection string 'Deke' is required.");

builder.Services.AddDekeInfrastructure(connectionString);
builder.Services.AddDekeEmbeddings(builder.Configuration);
builder.Services.AddDekeHarvesters();
builder.Services.AddDekeLlm(builder.Configuration);
builder.Services.AddDekeFederation(builder.Configuration);

builder.Services.AddHostedService<SourceMonitorService>();
builder.Services.AddHostedService<PatternDiscoveryService>();
builder.Services.AddHostedService<LearningCycleService>();
builder.Services.AddHostedService<PeerHealthCheckService>();

var host = builder.Build();
host.Run();
