using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace JsonApiDotNetCore.SourceGenerators;

internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static LocationInfo? TryCreateFrom(SyntaxNode node)
    {
        return TryCreateFrom(node.GetLocation());
    }

    private static LocationInfo? TryCreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }

    public override string ToString()
    {
        return ToLocation().ToString();
    }
}
