using Deke.Core.Interfaces;
using Deke.Infrastructure.Data;
using Deke.Infrastructure.Embeddings;
using Deke.Infrastructure.Extraction;
using Deke.Infrastructure.Harvesters;
using Deke.Infrastructure.Llm;
using Deke.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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
        services.AddSingleton<ILlmService, NoOpLlmService>();
        return services;
    }
}
