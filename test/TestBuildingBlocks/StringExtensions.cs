using JsonApiDotNetCore;

namespace TestBuildingBlocks;

public static class StringExtensions
{
    public static string DoubleQuote(this string source)
    {
        ArgumentGuard.NotNull(source, nameof(source));

        return '\"' + source + '\"';
    }
}
