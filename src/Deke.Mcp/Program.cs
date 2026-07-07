using System.Reflection;
using Deke.Infrastructure;
using Deke.Mcp.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Load user-secrets regardless of environment (Host only adds them in Development by default),
// so the Anthropic API key set via `dotnet user-secrets` is picked up when run as an MCP server.
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

// Serilog
builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

// Database + Embeddings
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? throw new InvalidOperationException("Connection string 'Deke' is required.");

builder.Services.AddDekeInfrastructure(connectionString);
builder.Services.AddDekeEmbeddings(builder.Configuration);
builder.Services.AddDekeLlm(builder.Configuration);
builder.Services.AddDekeFederation(builder.Configuration);
builder.Services.AddDekeAdvisory(builder.Configuration);

// MCP Server with stdio transport
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SearchTools>()
    .WithTools<FactTools>()
    .WithTools<AdvisoryTools>();

var host = builder.Build();
await host.RunAsync();
