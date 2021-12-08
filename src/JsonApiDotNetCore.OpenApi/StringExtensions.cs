using System.Text.RegularExpressions;

namespace JsonApiDotNetCore.OpenApi;

internal static class StringExtensions
{
    public static string Pascalize(this string source)
    {
        return Regex.Replace(source, "(?:^|-|_| +)(.)", match => match.Groups[1].Value.ToUpper());
    }
}
