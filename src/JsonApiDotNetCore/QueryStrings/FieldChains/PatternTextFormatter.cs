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
        ArgumentNullException.ThrowIfNull(pattern);

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
        if (types.HasFlag(FieldTypes.Field))
        {
            builder.Append('F');
            return;
        }

        int startOffset = builder.Length;

        if (types.HasFlag(FieldTypes.ToManyRelationship) && !types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('M');
        }

        if (types.HasFlag(FieldTypes.ToOneRelationship) && !types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('O');
        }

        if (types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('R');
        }

        if (types.HasFlag(FieldTypes.ScalarAttribute))
        {
            builder.Append('A');
        }

        if (types.HasFlag(FieldTypes.CompoundAttribute))
        {
            builder.Append('C');
        }

        if (types.HasFlag(FieldTypes.ScalarCollection))
        {
            builder.Append('L');
        }

        if (types.HasFlag(FieldTypes.CompoundCollection))
        {
            builder.Append('Q');
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
