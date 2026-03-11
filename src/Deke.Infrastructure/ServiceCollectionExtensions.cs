using Deke.Infrastructure.Data;
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
        return services;
    }
}
