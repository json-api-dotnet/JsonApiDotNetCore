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
        }
        else
        {
            int startOffset = builder.Length;

            WriteRelationshipTokens(types, builder);
            WriteAttributeTokens(types, builder);

            if (builder.Length - startOffset > 1)
            {
                builder.Insert(startOffset, '[');
                builder.Append(']');
            }
        }
    }

    private static void WriteRelationshipTokens(FieldTypes types, StringBuilder builder)
    {
        if (types.HasFlag(FieldTypes.Relationship))
        {
            builder.Append('R');
        }
        else if (types.HasFlag(FieldTypes.ToOneRelationship))
        {
            builder.Append('O');
        }
        else if (types.HasFlag(FieldTypes.ToManyRelationship))
        {
            builder.Append('M');
        }
    }

    private static void WriteAttributeTokens(FieldTypes types, StringBuilder builder)
    {
        if (types.HasFlag(FieldTypes.Attribute))
        {
            builder.Append('A');
        }
        else
        {
            if (types.HasFlag(FieldTypes.ScalarAttribute))
            {
                builder.Append('S');
            }

            if (types.HasFlag(FieldTypes.ScalarCollectionAttribute))
            {
                builder.Append('L');
            }

            if (types.HasFlag(FieldTypes.CompoundAttribute))
            {
                builder.Append('C');
            }

            if (types.HasFlag(FieldTypes.CompoundCollectionAttribute))
            {
                builder.Append('Q');
            }
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
