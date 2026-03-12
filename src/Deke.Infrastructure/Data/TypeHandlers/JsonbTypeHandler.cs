using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Deke.Infrastructure.Data.TypeHandlers;

public class JsonbTypeHandler<T> : SqlMapper.TypeHandler<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value, Options);
        parameter.DbType = DbType.String;
        if (parameter is NpgsqlParameter npgsqlParam)
        {
            npgsqlParam.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }

    public override T? Parse(object value)
    {
        if (value is null or DBNull)
            return default;

        var json = value.ToString()!;
        return JsonSerializer.Deserialize<T>(json, Options);
    }
}
