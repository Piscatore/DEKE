using Deke.Api.Endpoints;
using Deke.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Database
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? throw new InvalidOperationException("Connection string 'Deke' is required.");

builder.Services.AddDekeInfrastructure(connectionString);
builder.Services.AddDekeEmbeddings(builder.Configuration);

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapSearchEndpoints();
app.MapFactEndpoints();
app.MapSourceEndpoints();

app.Run();
