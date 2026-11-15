using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace JsonApiDotNetCore.SourceGenerators;

internal readonly record struct LocationInfo(SyntaxTree SyntaxTree, TextSpan TextSpan)
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

        return new LocationInfo(location.SourceTree, location.SourceSpan);
    }

    public Location ToLocation()
    {
        return Location.Create(SyntaxTree, TextSpan);
    }

    public override string ToString()
    {
        return ToLocation().ToString();
    }
}
