using System.Text;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Formats a chain of <see cref="FieldChainPattern" /> segments into text.
/// </summary>
internal sealed class PatternTextFormatter
{
    private readonly FieldChainPattern _pattern;

    public PatternTextFormatter(FieldChainPattern pattern)
    {
        ArgumentGuard.NotNull(pattern);

        _pattern = pattern;
    }

    public string Format()
    {
        FieldChainPattern? current = _pattern;
        var builder = new StringBuilder();

        do
        {
            WriteChoices(current.Choices, builder);
            WriteQuantifier(current.AtLeastOne, current.AtMostOne, builder);

            current = current.Next;
        }
        while (current != null);

        return builder.ToString();
    }

    private static void WriteChoices(FieldTypes types, StringBuilder builder)
    {
        int startOffset = builder.Length;

        if (types.HasFlag(FieldTypes.ToManyRelationship) && !types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('M');
        }

        if (types.HasFlag(FieldTypes.ToOneRelationship) && !types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('O');
        }

        if (types.HasFlag(FieldTypes.Attribute) && !types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('A');
        }

        if (types.HasFlag(FieldTypes.Relationship) && !types.HasFlag(FieldTypes.Field))
        {
            builder.Append('R');
        }

        if (types.HasFlag(FieldTypes.Field))
        {
            builder.Append('F');
        }

        int charCount = builder.Length - startOffset;

        if (charCount > 1)
        {
            builder.Insert(startOffset, '[');
            builder.Append(']');
        }
    }

    private static void WriteQuantifier(bool atLeastOne, bool atMostOne, StringBuilder builder)
    {
        if (!atLeastOne)
        {
            builder.Append(atMostOne ? '?' : '*');
        }
        else if (!atMostOne)
        {
            builder.Append('+');
        }
    }
}
