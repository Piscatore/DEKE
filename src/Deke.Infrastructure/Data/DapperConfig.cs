using Dapper;
using Dapper.FastCrud;
using Deke.Core.Models;
using Deke.Infrastructure.Data.TypeHandlers;
using Npgsql;
using Pgvector.Dapper;

namespace Deke.Infrastructure.Data;

public static class DapperConfig
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;

        // Set Dapper.FastCrud to use PostgreSQL dialect
        OrmConfiguration.DefaultDialect = SqlDialect.PostgreSql;

        // Register pgvector type handler
        SqlMapper.AddTypeHandler(new VectorTypeHandler());

        // Register JSONB type handlers
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<List<ExtractedEntity>>());
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<Dictionary<string, object>>());
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<List<TermContext>>());
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<Dictionary<string, string>>());

        // Register UUID[] type handler
        SqlMapper.AddTypeHandler(new GuidArrayTypeHandler());

        // Dapper: map underscore columns to PascalCase properties
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        _initialized = true;
    }

    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();
        return builder.Build();
    }
}
