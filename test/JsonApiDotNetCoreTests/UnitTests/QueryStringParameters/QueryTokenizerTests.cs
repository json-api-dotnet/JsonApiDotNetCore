using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

public sealed class QueryTokenizerTests
{
    [Theory]
    [InlineData("_", "Underscore at 0")]
    [InlineData("some_field", "Text: 'some_field' at 0")]
    [InlineData("a_", "Text: 'a_' at 0")]
    [InlineData("a_b.c_d", "Text: 'a_b' at 0|Period at 3|Text: 'c_d' at 4")]
    [InlineData("'a_b'", "QuotedText: 'a_b' at 0")]
    [InlineData("'_'", "QuotedText: '_' at 0")]
    [InlineData("equals(_,'a_b')", "Text: 'equals' at 0|OpenParen at 6|Underscore at 7|Comma at 8|QuotedText: 'a_b' at 9|CloseParen at 14")]
    [InlineData("has(tags,equals(_,'x'))",
        "Text: 'has' at 0|OpenParen at 3|Text: 'tags' at 4|Comma at 8|Text: 'equals' at 9|OpenParen at 15|Underscore at 16|Comma at 17|QuotedText: 'x' at 18|" +
        "CloseParen at 21|CloseParen at 22")]
    [InlineData("a.b.c", "Text: 'a' at 0|Period at 1|Text: 'b' at 2|Period at 3|Text: 'c' at 4")]
    [InlineData("-a", "Minus at 0|Text: 'a' at 1")]
    [InlineData("a-b", "Text: 'a-b' at 0")]
    [InlineData("_-x", "Text: '_-x' at 0")]
    [InlineData("__", "Text: '__' at 0")]
    [InlineData("a__b", "Text: 'a__b' at 0")]
    [InlineData("_'x'", "Underscore at 0|QuotedText: 'x' at 1")]
    [InlineData("(_)", "OpenParen at 0|Underscore at 1|CloseParen at 2")]
    [InlineData("_.x", "Underscore at 0|Period at 1|Text: 'x' at 2")]
    [InlineData("equals(_,_)", "Text: 'equals' at 0|OpenParen at 6|Underscore at 7|Comma at 8|Underscore at 9|CloseParen at 10")]
    public void Tokenizes_Input(string source, string tokensExpected)
    {
        // Arrange
        var tokenizer = new QueryTokenizer(source);

        // Act
        Token[] tokens = tokenizer.EnumerateTokens().ToArray();

        // Assert
        string.Join('|', tokens.Select(token => token.ToString())).Should().Be(tokensExpected);
    }

    [Fact]
    public void Tokenizes_Text_Starting_With_Underscore_As_Single_Token()
    {
        // Arrange
        var tokenizer = new QueryTokenizer("_x");

        // Act
        Token[] tokens = tokenizer.EnumerateTokens().ToArray();

        // Assert
        string.Join('|', tokens.Select(token => token.ToString())).Should().Be("Text: '_x' at 0");
    }

    [Fact]
    public void Tokenizes_Text_Starting_With_Underscore_After_Period_As_Single_Token()
    {
        // Arrange
        var tokenizer = new QueryTokenizer("a._b");

        // Act
        Token[] tokens = tokenizer.EnumerateTokens().ToArray();

        // Assert
        string.Join('|', tokens.Select(token => token.ToString())).Should().Be("Text: 'a' at 0|Period at 1|Text: '_b' at 2");
    }

    [Theory]
    [InlineData("'abc", "Closing ' expected.", 3)]
    [InlineData("equals(a,'abc", "Closing ' expected.", 12)]
    [InlineData("abc'", "Unexpected ' outside quoted text.", 3)]
    [InlineData("a.b'c'", "Unexpected ' outside quoted text.", 3)]
    [InlineData("a b", "Unexpected whitespace.", 1)]
    [InlineData("_ x", "Unexpected whitespace.", 1)]
    [InlineData("'x' _", "Unexpected whitespace.", 3)]
    [InlineData("has(tags,equals( _,'x'))", "Unexpected whitespace.", 16)]
    [InlineData("_'x", "Closing ' expected.", 2)]
    public void Fails_On_Invalid_Input(string source, string errorMessage, int position)
    {
        // Arrange
        var tokenizer = new QueryTokenizer(source);

        // Act
        Action action = () => _ = tokenizer.EnumerateTokens().ToArray();

        // Assert
        QueryParseException exception = action.Should().ThrowExactly<QueryParseException>().Which;
        exception.Message.Should().Be(errorMessage);
        exception.Position.Should().Be(position);
    }
}
