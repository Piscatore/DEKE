using Deke.Core.Interfaces;
using Deke.Infrastructure.Data;
using Deke.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Deke.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDekeInfrastructure(this IServiceCollection services, string connectionString)
    {
        DapperConfig.Initialize();
        var dataSource = DapperConfig.CreateDataSource(connectionString);
        services.AddSingleton(dataSource);
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
}
