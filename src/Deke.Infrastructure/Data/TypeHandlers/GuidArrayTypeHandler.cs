using System.Data;
using Dapper;

namespace Deke.Infrastructure.Data.TypeHandlers;

public class GuidArrayTypeHandler : SqlMapper.TypeHandler<List<Guid>>
{
    public override void SetValue(IDbDataParameter parameter, List<Guid>? value)
    {
        parameter.Value = value?.ToArray() ?? Array.Empty<Guid>();
    }

    public override List<Guid> Parse(object value)
    {
        if (value is null or DBNull)
            return [];

        if (value is Guid[] array)
            return [.. array];

        return [];
    }
}
