using Deke.Infrastructure.Data;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(config =>
    config.ReadFrom.Configuration(builder.Configuration));

// Database
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? "Host=localhost;Database=deke;Username=deke;Password=deke";

DapperConfig.Initialize();
var dataSource = DapperConfig.CreateDataSource(connectionString);
builder.Services.AddSingleton(dataSource);
builder.Services.AddSingleton<DbConnectionFactory>();

var host = builder.Build();
host.Run();
