using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class QueryParseChoiceFormatterTests
{
    [Theory]
    [InlineData(QueryParseChoices.Minus, "- expected.")]
    [InlineData(QueryParseChoices.OpenParenthesis, "( expected.")]
    [InlineData(QueryParseChoices.FilterFunction, "Filter function expected.")]
    [InlineData(QueryParseChoices.CountFunction, "count function expected.")]
    [InlineData(QueryParseChoices.Function, "Function expected.")]
    [InlineData(QueryParseChoices.Number, "Number expected.")]
    [InlineData(QueryParseChoices.ParameterName, "Parameter name expected.")]
    [InlineData(QueryParseChoices.ResourceType, "Resource type expected.")]
    [InlineData(QueryParseChoices.FieldName, "Field name expected.")]
    [InlineData(QueryParseChoices.ToOneRelationshipName, "To-one relationship name expected.")]
    [InlineData(QueryParseChoices.ToManyRelationshipName, "To-many relationship name expected.")]
    [InlineData(QueryParseChoices.RelationshipName, "Relationship name expected.")]
    [InlineData(QueryParseChoices.QuotedText, "Value between quotes expected.")]
    [InlineData(QueryParseChoices.Underscore, "_ expected.")]
    [InlineData(QueryParseChoices.Null, "null expected.")]
    [InlineData(QueryParseChoices.Comma, ", expected.")]
    [InlineData(QueryParseChoices.EndOfExpression, "End of expression expected.")]
    public void Formats_each_single_choice(QueryParseChoices choice, string messageExpected)
    {
        // Act
        string messageActual = QueryParseChoiceFormatter.Format(choice);

        // Assert
        messageActual.Should().Be(messageExpected);
    }

    [Theory]
    [InlineData(QueryParseChoices.Function | QueryParseChoices.FieldName, "Function or field name expected.")]
    [InlineData(QueryParseChoices.Function | QueryParseChoices.FieldName | QueryParseChoices.QuotedText,
        "Function, field name or value between quotes expected.")]
    [InlineData(QueryParseChoices.Function | QueryParseChoices.FieldName | QueryParseChoices.QuotedText | QueryParseChoices.Null,
        "Function, field name, value between quotes or null expected.")]
    public void Formats_combinations_of_varying_lengths(QueryParseChoices choices, string messageExpected)
    {
        // Act
        string messageActual = QueryParseChoiceFormatter.Format(choices);

        // Assert
        messageActual.Should().Be(messageExpected);
    }

    [Theory]
    [InlineData(QueryParseChoices.FieldName, "Field name expected.")]
    [InlineData(QueryParseChoices.Minus | QueryParseChoices.FieldName, "- or field name expected.")]
    [InlineData(QueryParseChoices.Null, "null expected.")]
    [InlineData(QueryParseChoices.Null | QueryParseChoices.Comma, "null or , expected.")]
    [InlineData(QueryParseChoices.CountFunction | QueryParseChoices.FieldName, "count function or field name expected.")]
    [InlineData(QueryParseChoices.FieldName | QueryParseChoices.Null, "Field name or null expected.")]
    [InlineData(QueryParseChoices.Minus | QueryParseChoices.CountFunction | QueryParseChoices.FieldName, "-, count function or field name expected.")]
    public void Handles_casing_and_symbols_at_first_and_subsequent_positions(QueryParseChoices choices, string messageExpected)
    {
        // Act
        string messageActual = QueryParseChoiceFormatter.Format(choices);

        // Assert
        messageActual.Should().Be(messageExpected);
    }

    [Theory]
    [InlineData(QueryParseChoices.ToOneRelationshipName | QueryParseChoices.ToManyRelationshipName, "Relationship name expected.")]
    [InlineData(QueryParseChoices.Number | QueryParseChoices.ToOneRelationshipName | QueryParseChoices.ToManyRelationshipName,
        "Number or relationship name expected.")]
    public void Collapses_relationship_choices(QueryParseChoices choices, string messageExpected)
    {
        // Act
        string messageActual = QueryParseChoiceFormatter.Format(choices);

        // Assert
        messageActual.Should().Be(messageExpected);
    }

    [Fact]
    public void Formats_only_custom_choices()
    {
        // Act
        string message = QueryParseChoiceFormatter.Format(QueryParseChoices.None, "first", "second", "third");

        // Assert
        message.Should().Be("first, second or third expected.");
    }

    [Fact]
    public void Formats_built_in_combined_with_custom_choices()
    {
        // Act
        string message = QueryParseChoiceFormatter.Format(QueryParseChoices.Function | QueryParseChoices.FieldName, "one", "two");

        // Assert
        message.Should().Be("Function, field name, one or two expected.");
    }

    [Fact]
    public void Ignores_empty_or_whitespace_custom_choices()
    {
        // Act
        string message = QueryParseChoiceFormatter.Format(QueryParseChoices.FieldName, string.Empty, "   ", "some");

        // Assert
        message.Should().Be("Field name or some expected.");
    }

    [Fact]
    public void Formats_empty_choices_to_empty_string()
    {
        // Act
        string messageNone = QueryParseChoiceFormatter.Format(QueryParseChoices.None);

        // Assert
        messageNone.Should().BeEmpty();
    }
}
