using System.Text;
using JsonApiDotNetCore.Resources;

namespace DapperExample.TranslationToSql;

/// <summary>
/// Converts a SQL parameter into human-readable text. Used for diagnostic purposes.
/// </summary>
internal sealed class ParameterFormatter
{
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(bool),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(sbyte),
        typeof(float),
        typeof(double),
        typeof(decimal)
    ];

    public string Format(string parameterName, object? parameterValue)
    {
        StringBuilder builder = new();
        builder.Append($"{parameterName} = ");
        WriteValue(parameterValue, builder);
        return builder.ToString();
    }

    private void WriteValue(object? parameterValue, StringBuilder builder)
    {
        if (parameterValue == null)
        {
            builder.Append("null");
        }
        else if (parameterValue is char)
        {
            builder.Append($"'{parameterValue}'");
        }
        else if (parameterValue is byte byteValue)
        {
            builder.Append($"0x{byteValue:X2}");
        }
        else if (parameterValue is Enum)
        {
            builder.Append($"{parameterValue.GetType().Name}.{parameterValue}");
        }
        else
        {
            string value = (string)RuntimeTypeConverter.ConvertType(parameterValue, typeof(string))!;

            if (NumericTypes.Contains(parameterValue.GetType()))
            {
                builder.Append(value);
            }
            else
            {
                string escapedValue = value.Replace("'", "''");
                builder.Append($"'{escapedValue}'");
            }
        }
    }
}
