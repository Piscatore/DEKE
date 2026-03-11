using Deke.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Database
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? "Host=localhost;Database=deke;Username=deke;Password=deke";

DapperConfig.Initialize();
var dataSource = DapperConfig.CreateDataSource(connectionString);
builder.Services.AddSingleton(dataSource);
builder.Services.AddSingleton<DbConnectionFactory>();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
