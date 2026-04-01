using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;
using Deke.Infrastructure.Embeddings;
using Deke.Infrastructure.Extraction;
using Deke.Infrastructure.Federation;
using Deke.Infrastructure.Harvesters;
using Deke.Infrastructure.Llm;
using Deke.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace Deke.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDekeInfrastructure(this IServiceCollection services, string connectionString)
    {
        DapperConfig.Initialize();
        services.AddSingleton<NpgsqlDataSource>(_ => DapperConfig.CreateDataSource(connectionString));
        services.AddSingleton<DbConnectionFactory>();

        // Repositories
        services.AddScoped<IFactRepository, FactRepository>();
        services.AddScoped<ITermRepository, TermRepository>();
        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<IPatternRepository, PatternRepository>();
        services.AddScoped<IFactRelationRepository, FactRelationRepository>();
        services.AddScoped<ILearningLogRepository, LearningLogRepository>();

        return services;
    }

    public static IServiceCollection AddDekeEmbeddings(
        this IServiceCollection services, IConfiguration configuration)
    {
        var config = new EmbeddingsConfig
        {
            ModelPath = configuration["Embeddings:ModelPath"] ?? "models/all-MiniLM-L6-v2/model.onnx",
            VocabPath = configuration["Embeddings:VocabPath"] ?? "models/all-MiniLM-L6-v2/vocab.txt"
        };

        services.AddSingleton(config);
        services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();

        return services;
    }

    public static IServiceCollection AddDekeHarvesters(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IHarvester, RssHarvester>();
        services.AddScoped<IHarvester, WebPageHarvester>();
        services.AddScoped<IExtractionService, SimpleExtractionService>();
        return services;
    }

    public static IServiceCollection AddDekeFederation(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FederationConfig>(configuration.GetSection("Federation"));

        services.AddScoped<IFederationPeerRepository, FederationPeerRepository>();
        services.AddSingleton<FederationClient>();
        services.AddScoped<IFederatedSearchService, FederatedSearchService>();

        services.AddHttpClient("federation", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Add("User-Agent", "DEKE/1.0");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);

            options.Retry.MaxRetryAttempts = 2;
            options.Retry.BackoffType = DelayBackoffType.Exponential;

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 3;

            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }

    public static IServiceCollection AddDekeLlm(
        this IServiceCollection services, IConfiguration configuration)
    {
        var config = new LlmConfig();
        configuration.GetSection("Llm").Bind(config);
        services.AddSingleton(config);

        switch (config.Provider)
        {
            case LlmProvider.Gemini:
                services.AddHttpClient<ILlmService, GeminiLlmService>();
                break;

            case LlmProvider.OpenAi:
                services.AddHttpClient<ILlmService, OpenAiLlmService>();
                break;

            default:
                services.AddSingleton<ILlmService, NoOpLlmService>();
                break;
        }

        return services;
    }
}
