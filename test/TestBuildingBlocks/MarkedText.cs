using System.Diagnostics;
using JetBrains.Annotations;
using JsonApiDotNetCore;

namespace TestBuildingBlocks;

[PublicAPI]
[DebuggerDisplay($"{{{nameof(Source)}}}")]
public sealed class MarkedText
{
    public string Source { get; }
    public int Position { get; }
    public string Text { get; }

    public MarkedText(string source, char marker)
    {
        ArgumentGuard.NotNull(source);

        Source = source;
        Position = GetPositionFromMarker(marker);
        Text = source.Replace(marker.ToString(), string.Empty);
    }

    private int GetPositionFromMarker(char marker)
    {
        int position = Source.IndexOf(marker);

        if (position == -1)
        {
            throw new InvalidOperationException("Marker not found.");
        }

        if (Source.IndexOf(marker, position + 1) != -1)
        {
            throw new InvalidOperationException("Multiple markers found.");
        }

        return position;
    }

    public override string ToString()
    {
        return $"Failed at position {Position + 1}: {Source}";
    }
}
