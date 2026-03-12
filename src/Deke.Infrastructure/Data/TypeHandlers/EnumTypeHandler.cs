using System.Data;
using Dapper;

namespace Deke.Infrastructure.Data.TypeHandlers;

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }

    public override T Parse(object value)
    {
        return Enum.Parse<T>(value.ToString()!, ignoreCase: true);
    }
}
