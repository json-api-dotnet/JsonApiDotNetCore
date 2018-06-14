using System.Text.RegularExpressions;

namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{

    public static class StringExtensions
    {
        public static string Normalize(this string input)
        {
            return Regex.Replace(input, @"\s+", string.Empty)
                .ToUpper()
                .Replace("\"", string.Empty)
                .Replace("'", string.Empty);
        }
    }
}
