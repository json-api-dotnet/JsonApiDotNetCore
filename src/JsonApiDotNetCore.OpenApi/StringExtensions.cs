using System.Text.RegularExpressions;

namespace JsonApiDotNetCore.OpenApi;

internal static class StringExtensions
{
    private static readonly Regex PatternForPascalCaseConversion = new("(?:^|-|_| +)(.)", RegexOptions.Compiled);

    public static string ToPascalCase(this string source)
    {
        return PatternForPascalCaseConversion.Replace(source, match => match.Groups[1].Value.ToUpper());
    }
}
