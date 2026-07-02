using System.Data;
using Dapper;

namespace Deke.Infrastructure.Data.TypeHandlers;

public class FloatArrayTypeHandler : SqlMapper.TypeHandler<List<float>>
{
    public override void SetValue(IDbDataParameter parameter, List<float>? value)
    {
        parameter.Value = value?.ToArray() ?? Array.Empty<float>();
    }

    public override List<float> Parse(object value)
    {
        if (value is null or DBNull)
            return [];

        if (value is float[] array)
            return [.. array];

        return [];
    }
}
