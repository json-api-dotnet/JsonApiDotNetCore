using System.Text.RegularExpressions;

namespace JsonApiDotNetCore.OpenApi;

internal static class StringExtensions
{
    private static readonly Regex Pattern = new("(?:^|-|_| +)(.)", RegexOptions.Compiled);

    public static string ToPascalCase(this string source)
    {
        return Pattern.Replace(source, match => match.Groups[1].Value.ToUpper());
    }
}
