using System.Text;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Formats a set of <see cref="QueryParseChoices" /> and optional custom choices into a human-readable error message.
/// </summary>
internal static class QueryParseChoiceFormatter
{
    public static string Format(QueryParseChoices choices, params ReadOnlySpan<string> customChoices)
    {
        List<ChoiceValue> choiceList = [];

        if (choices.HasFlag(QueryParseChoices.Minus))
        {
            choiceList.Add(new ChoiceValue("-"));
        }

        if (choices.HasFlag(QueryParseChoices.OpenParenthesis))
        {
            choiceList.Add(new ChoiceValue("("));
        }

        if (choices.HasFlag(QueryParseChoices.FilterFunction))
        {
            choiceList.Add(new ChoiceValue("filter function"));
        }

        if (choices.HasFlag(QueryParseChoices.CountFunction))
        {
            choiceList.Add(new ChoiceValue("count function", true));
        }

        if (choices.HasFlag(QueryParseChoices.Function))
        {
            choiceList.Add(new ChoiceValue("function"));
        }

        if (choices.HasFlag(QueryParseChoices.Number))
        {
            choiceList.Add(new ChoiceValue("number"));
        }

        if (choices.HasFlag(QueryParseChoices.ParameterName))
        {
            choiceList.Add(new ChoiceValue("parameter name"));
        }

        if (choices.HasFlag(QueryParseChoices.ResourceType))
        {
            choiceList.Add(new ChoiceValue("resource type"));
        }

        if (choices.HasFlag(QueryParseChoices.FieldName))
        {
            choiceList.Add(new ChoiceValue("field name"));
        }

        if (choices.HasFlag(QueryParseChoices.RelationshipName))
        {
            choiceList.Add(new ChoiceValue("relationship name"));
        }
        else if (choices.HasFlag(QueryParseChoices.ToOneRelationshipName))
        {
            choiceList.Add(new ChoiceValue("to-one relationship name"));
        }
        else if (choices.HasFlag(QueryParseChoices.ToManyRelationshipName))
        {
            choiceList.Add(new ChoiceValue("to-many relationship name"));
        }

        if (choices.HasFlag(QueryParseChoices.QuotedText))
        {
            choiceList.Add(new ChoiceValue("value between quotes"));
        }

        if (choices.HasFlag(QueryParseChoices.Underscore))
        {
            choiceList.Add(new ChoiceValue("_"));
        }

        if (choices.HasFlag(QueryParseChoices.Null))
        {
            choiceList.Add(new ChoiceValue(Keywords.Null, true));
        }

        if (choices.HasFlag(QueryParseChoices.Comma))
        {
            choiceList.Add(new ChoiceValue(","));
        }

        if (choices.HasFlag(QueryParseChoices.EndOfExpression))
        {
            choiceList.Add(new ChoiceValue("end of expression"));
        }

        foreach (string customChoice in customChoices)
        {
            if (!string.IsNullOrWhiteSpace(customChoice))
            {
                choiceList.Add(new ChoiceValue(customChoice, true));
            }
        }

        return FormatChoices(choiceList);
    }

    private static string FormatChoices(List<ChoiceValue> choices)
    {
        var builder = new StringBuilder();

        if (choices.Count > 0)
        {
            for (int index = 0; index < choices.Count; index++)
            {
                ChoiceValue choiceValue = choices[index];

                if (index == 0)
                {
                    if (choiceValue.PreserveCase)
                    {
                        builder.Append(choiceValue.Text);
                    }
                    else
                    {
                        builder.Append(char.ToUpperInvariant(choiceValue.Text[0]));
                        builder.Append(choiceValue.Text.AsSpan(1));
                    }
                }
                else if (index == choices.Count - 1)
                {
                    builder.Append(" or ");
                    builder.Append(choiceValue.Text);
                }
                else
                {
                    builder.Append(", ");
                    builder.Append(choiceValue.Text);
                }
            }

            builder.Append(" expected.");
        }

        return builder.ToString();
    }

    private readonly record struct ChoiceValue(string Text, bool PreserveCase = false);
}
