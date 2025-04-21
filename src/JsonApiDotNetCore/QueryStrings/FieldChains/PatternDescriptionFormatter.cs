using System.Text;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Formats a chain of <see cref="FieldChainPattern" /> segments into a human-readable description.
/// </summary>
internal sealed class PatternDescriptionFormatter
{
    private readonly FieldChainPattern _pattern;

    public PatternDescriptionFormatter(FieldChainPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        _pattern = pattern;
    }

    public string Format()
    {
        FieldChainPattern? current = _pattern;
        var builder = new StringBuilder();

        do
        {
            WriteSeparator(builder);
            WriteQuantifier(current.AtLeastOne, current.AtMostOne, builder);
            WriteChoices(current, builder);

            current = current.Next;
        }
        while (current != null);

        return builder.ToString();
    }

    private static void WriteSeparator(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.Append(", followed by ");
        }
    }

    private static void WriteQuantifier(bool atLeastOne, bool atMostOne, StringBuilder builder)
    {
        if (!atLeastOne)
        {
            builder.Append(atMostOne ? "an optional " : "zero or more ");
        }
        else if (!atMostOne)
        {
            builder.Append("one or more ");
        }
    }

    private static void WriteChoices(FieldChainPattern pattern, StringBuilder builder)
    {
        bool pluralize = !pattern.AtMostOne;
        bool prefix = pattern is { AtLeastOne: true, AtMostOne: true };

        pattern.Choices.WriteTo(builder, pluralize, prefix);
    }
}
