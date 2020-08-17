namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
