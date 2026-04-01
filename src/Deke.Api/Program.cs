using Deke.Api.Auth;
using Deke.Api.Endpoints;
using Deke.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Local overrides (gitignored)
var env = builder.Environment;
builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Database
var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? throw new InvalidOperationException("Connection string 'Deke' is required.");

builder.Services.AddDekeInfrastructure(connectionString);
builder.Services.AddDekeEmbeddings(builder.Configuration);
builder.Services.AddDekeLlm(builder.Configuration);
builder.Services.AddDekeFederation(builder.Configuration);

// Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>("ApiKey", null);
builder.Services.AddAuthorizationBuilder()
    .AddFallbackPolicy("AuthenticatedOnly", policy => policy.RequireAuthenticatedUser());

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.MapSearchEndpoints();
app.MapFactEndpoints();
app.MapSourceEndpoints();
app.MapFederationEndpoints();

app.Run();
