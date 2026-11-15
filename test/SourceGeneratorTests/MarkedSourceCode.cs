using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorTests;

/// <summary>
/// Extracts markers from source code into a diagnostic span.
/// <example>
/// For example:
/// <code><![CDATA[
/// var source = new MarkedSourceCode("public class [|Item|];");
/// Console.WriteLine(source.Text); // "public class Item;"
/// Console.WriteLine(source.DiagnosticSpan); // TextSpan(12, 4)
/// ]]></code>
/// </example>
/// </summary>
internal sealed class MarkedSourceCode
{
    private const string MarkerStartToken = "[|";
    private const string MarkerStopToken = "|]";

    public string Text { get; }
    public TextSpan DiagnosticSpan { get; }

    public MarkedSourceCode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReadOnlySpan<char> textSpan = text.AsSpan();
        int markerStartIndex = textSpan.IndexOf(MarkerStartToken.AsSpan(), StringComparison.Ordinal);

        if (markerStartIndex == -1)
        {
            Text = text;
        }
        else
        {
            int markedTextStart = markerStartIndex + MarkerStartToken.Length;
            int markerStopIndex = textSpan[markedTextStart..].IndexOf(MarkerStopToken.AsSpan(), StringComparison.Ordinal);

            if (markerStopIndex == -1)
            {
                throw new FormatException($"Missing {MarkerStopToken}");
            }

            markerStopIndex += markedTextStart;
            int markedTextLength = markerStopIndex - markedTextStart;

            if (markedTextLength < 1)
            {
                throw new FormatException("Text between markers cannot be empty.");
            }

            ReadOnlySpan<char> leftText = textSpan[..markerStartIndex];
            ReadOnlySpan<char> middleText = textSpan.Slice(markedTextStart, markedTextLength);
            ReadOnlySpan<char> rightText = textSpan[(markerStopIndex + MarkerStopToken.Length)..];
            Text = $"{leftText}{middleText}{rightText}";

            DiagnosticSpan = new TextSpan(markerStartIndex, markedTextLength);
        }
    }

    public override string ToString()
    {
        if (!DiagnosticSpan.IsEmpty)
        {
            ReadOnlySpan<char> textSpan = Text.AsSpan();

            ReadOnlySpan<char> leftText = textSpan[..DiagnosticSpan.Start];
            ReadOnlySpan<char> middleText = textSpan.Slice(DiagnosticSpan.Start, DiagnosticSpan.Length);
            ReadOnlySpan<char> rightText = textSpan[DiagnosticSpan.End..];

            return $"{leftText}{MarkerStartToken.AsSpan()}{middleText}{MarkerStopToken.AsSpan()}{rightText}";
        }

        return Text;
    }
}
