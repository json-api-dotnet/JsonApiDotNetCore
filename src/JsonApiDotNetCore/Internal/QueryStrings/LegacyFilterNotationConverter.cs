using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public sealed class LegacyFilterNotationConverter
    {
        private const string _parameterNamePrefix = "filter[";
        private const string _parameterNameSuffix = "]";
        private const string _outputParameterName = "filter";

        private const string _expressionPrefix = "expr:";
        private const string _notEqualsPrefix = "ne:";
        private const string _inPrefix = "in:";
        private const string _notInPrefix = "nin:";

        private static readonly Dictionary<string, string> _prefixConversionTable = new Dictionary<string, string>
        {
            ["eq:"] = Keywords.Equals,
            ["lt:"] = Keywords.LessThan,
            ["le:"] = Keywords.LessOrEqual,
            ["gt:"] = Keywords.GreaterThan,
            ["ge:"] = Keywords.GreaterOrEqual,
            ["like:"] = Keywords.Contains
        };

        public (string parameterName, string parameterValue) Convert(string parameterName, string parameterValue)
        {
            if (parameterValue.StartsWith(_expressionPrefix))
            {
                string expression = parameterValue.Substring(_expressionPrefix.Length);
                return (parameterName, expression);
            }

            var attributeName = ExtractAttributeName(parameterName);

            foreach (var (prefix, keyword) in _prefixConversionTable)
            {
                if (parameterValue.StartsWith(prefix))
                {
                    var value = parameterValue.Substring(prefix.Length);
                    string escapedValue = EscapeQuotes(value);
                    string expression = $"{keyword}({attributeName},'{escapedValue}')";

                    return (_outputParameterName, expression);
                }
            }

            if (parameterValue.StartsWith(_notEqualsPrefix))
            {
                var value = parameterValue.Substring(_notEqualsPrefix.Length);
                string escapedValue = EscapeQuotes(value);
                string expression = $"{Keywords.Not}({Keywords.Equals}({attributeName},'{escapedValue}'))";

                return (_outputParameterName, expression);
            }

            if (parameterValue.StartsWith(_inPrefix))
            {
                string[] valueParts = parameterValue.Substring(_inPrefix.Length).Split(",");
                var valueList = "'" + string.Join("','", valueParts) + "'";
                string expression = $"{Keywords.Any}({attributeName},{valueList})";

                return (_outputParameterName, expression);
            }

            if (parameterValue.StartsWith(_notInPrefix))
            {
                string[] valueParts = parameterValue.Substring(_notInPrefix.Length).Split(",");
                var valueList = "'" + string.Join("','", valueParts) + "'";
                string expression = $"{Keywords.Not}({Keywords.Any}({attributeName},{valueList}))";

                return (_outputParameterName, expression);
            }

            if (parameterValue == "isnull:")
            {
                string expression = $"{Keywords.Equals}({attributeName},null)";
                return (_outputParameterName, expression);
            }

            if (parameterValue == "isnotnull:")
            {
                string expression = $"{Keywords.Not}({Keywords.Equals}({attributeName},null))";
                return (_outputParameterName, expression);
            }

            {
                string escapedValue = EscapeQuotes(parameterValue);
                string expression = $"{Keywords.Equals}({attributeName},'{escapedValue}')";

                return (_outputParameterName, expression);
            }
        }

        private static string ExtractAttributeName(string parameterName)
        {
            if (parameterName.StartsWith(_parameterNamePrefix) && parameterName.EndsWith(_parameterNameSuffix))
            {
                string attributeName = parameterName.Substring(_parameterNamePrefix.Length,
                    parameterName.Length - _parameterNamePrefix.Length - _parameterNameSuffix.Length);

                if (attributeName.Length > 0)
                {
                    return attributeName;
                }
            }

            throw new QueryParseException("Expected field name between brackets in filter parameter name.");
        }

        private static string EscapeQuotes(string text)
        {
            return text.Replace("'", "''");
        }
    }
}
