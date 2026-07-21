using Deke.Infrastructure;
using Deke.Infrastructure.Advisory;
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
builder.Services.AddDekeDedup(builder.Configuration);
builder.Services.AddDekeFederation(builder.Configuration);

// PatternDiscoveryService summarizes fact clusters via the same keyed IChatClient
// backends the Advisory pipeline uses (ollama key -- local/cheap, matches its
// hourly-batch cost/latency profile per ADR-0007).
var advisoryConfig = new AdvisoryConfig();
builder.Configuration.GetSection("Advisory").Bind(advisoryConfig);
builder.Services.AddSingleton(advisoryConfig);
builder.Services.AddAdvisoryChatClients(advisoryConfig);

builder.Services.AddScoped<BootstrapIngestionService>();

if (args.Contains("--bootstrap"))
{
    var bootstrapApp = builder.Build();

    using var scope = bootstrapApp.Services.CreateScope();
    var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapIngestionService>();
    var repoRoot = args.FirstOrDefault(a => !a.StartsWith("--")) ?? Directory.GetCurrentDirectory();

    await bootstrap.RunAsync(repoRoot);
    return;
}

builder.Services.AddHostedService<SourceMonitorService>();
builder.Services.AddHostedService<PatternDiscoveryService>();
builder.Services.AddHostedService<LearningCycleService>();
builder.Services.AddHostedService<PeerHealthCheckService>();
builder.Services.AddHostedService<SimilarityDedupService>();
builder.Services.AddHostedService<SemanticDedupService>();

var host = builder.Build();
host.Run();
