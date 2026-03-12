using Npgsql;

namespace Deke.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken ct = default)
    {
        var conn = _dataSource.CreateConnection();
        await conn.OpenAsync(ct);
        return conn;
    }
}
