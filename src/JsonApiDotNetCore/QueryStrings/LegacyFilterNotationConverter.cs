using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.QueryStrings;

[PublicAPI]
public sealed class LegacyFilterNotationConverter
{
    private const string ParameterNamePrefix = "filter[";
    private const string ParameterNameSuffix = "]";
    private const string OutputParameterName = "filter";

    private static readonly Dictionary<string, string> PrefixConversionTable = new()
    {
        [ParameterValuePrefix.Equal] = Keywords.Equals,
        [ParameterValuePrefix.LessThan] = Keywords.LessThan,
        [ParameterValuePrefix.LessOrEqual] = Keywords.LessOrEqual,
        [ParameterValuePrefix.GreaterThan] = Keywords.GreaterThan,
        [ParameterValuePrefix.GreaterEqual] = Keywords.GreaterOrEqual,
        [ParameterValuePrefix.Like] = Keywords.Contains
    };

    public IEnumerable<string> ExtractConditions(string parameterValue)
    {
        ArgumentGuard.NotNullNorEmpty(parameterValue);

        if (parameterValue.StartsWith(ParameterValuePrefix.Expression, StringComparison.Ordinal) ||
            parameterValue.StartsWith(ParameterValuePrefix.In, StringComparison.Ordinal) ||
            parameterValue.StartsWith(ParameterValuePrefix.NotIn, StringComparison.Ordinal))
        {
            yield return parameterValue;
        }
        else
        {
            foreach (string condition in parameterValue.Split(','))
            {
                yield return condition;
            }
        }
    }

    public (string parameterName, string parameterValue) Convert(string parameterName, string parameterValue)
    {
        ArgumentGuard.NotNullNorEmpty(parameterName);
        ArgumentGuard.NotNullNorEmpty(parameterValue);

        if (parameterValue.StartsWith(ParameterValuePrefix.Expression, StringComparison.Ordinal))
        {
            string expression = parameterValue[ParameterValuePrefix.Expression.Length..];
            return (parameterName, expression);
        }

        string attributeName = ExtractAttributeName(parameterName);

        foreach ((string prefix, string keyword) in PrefixConversionTable)
        {
            if (parameterValue.StartsWith(prefix, StringComparison.Ordinal))
            {
                string value = parameterValue[prefix.Length..];
                string escapedValue = EscapeQuotes(value);
                string expression = $"{keyword}({attributeName},'{escapedValue}')";

                return (OutputParameterName, expression);
            }
        }

        if (parameterValue.StartsWith(ParameterValuePrefix.NotEqual, StringComparison.Ordinal))
        {
            string value = parameterValue[ParameterValuePrefix.NotEqual.Length..];
            string escapedValue = EscapeQuotes(value);
            string expression = $"{Keywords.Not}({Keywords.Equals}({attributeName},'{escapedValue}'))";

            return (OutputParameterName, expression);
        }

        if (parameterValue.StartsWith(ParameterValuePrefix.In, StringComparison.Ordinal))
        {
            string[] valueParts = parameterValue[ParameterValuePrefix.In.Length..].Split(",");
            string valueList = $"'{string.Join("','", valueParts)}'";
            string expression = $"{Keywords.Any}({attributeName},{valueList})";

            return (OutputParameterName, expression);
        }

        if (parameterValue.StartsWith(ParameterValuePrefix.NotIn, StringComparison.Ordinal))
        {
            string[] valueParts = parameterValue[ParameterValuePrefix.NotIn.Length..].Split(",");
            string valueList = $"'{string.Join("','", valueParts)}'";
            string expression = $"{Keywords.Not}({Keywords.Any}({attributeName},{valueList}))";

            return (OutputParameterName, expression);
        }

        if (parameterValue == ParameterValuePrefix.IsNull)
        {
            string expression = $"{Keywords.Equals}({attributeName},null)";
            return (OutputParameterName, expression);
        }

        if (parameterValue == ParameterValuePrefix.IsNotNull)
        {
            string expression = $"{Keywords.Not}({Keywords.Equals}({attributeName},null))";
            return (OutputParameterName, expression);
        }

        {
            string escapedValue = EscapeQuotes(parameterValue);
            string expression = $"{Keywords.Equals}({attributeName},'{escapedValue}')";

            return (OutputParameterName, expression);
        }
    }

    private static string ExtractAttributeName(string parameterName)
    {
        if (parameterName.StartsWith(ParameterNamePrefix, StringComparison.Ordinal) && parameterName.EndsWith(ParameterNameSuffix, StringComparison.Ordinal))
        {
            string attributeName = parameterName.Substring(ParameterNamePrefix.Length,
                parameterName.Length - ParameterNamePrefix.Length - ParameterNameSuffix.Length);

            if (attributeName.Length > 0)
            {
                return attributeName;
            }
        }

        throw new QueryParseException("Expected field name between brackets in filter parameter name.", -1);
    }

    private static string EscapeQuotes(string text)
    {
        return text.Replace("'", "''");
    }

    private sealed class ParameterValuePrefix
    {
        public const string Equal = "eq:";
        public const string NotEqual = "ne:";
        public const string LessThan = "lt:";
        public const string LessOrEqual = "le:";
        public const string GreaterThan = "gt:";
        public const string GreaterEqual = "ge:";
        public const string Like = "like:";
        public const string In = "in:";
        public const string NotIn = "nin:";
        public const string IsNull = "isnull:";
        public const string IsNotNull = "isnotnull:";
        public const string Expression = "expr:";
    }
}
