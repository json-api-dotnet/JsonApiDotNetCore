using FluentAssertions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.FieldChains;

public sealed class FieldChainPatternParseTests
{
    [Theory]
    [InlineData("M", "a to-many relationship")]
    [InlineData("MO", "a to-many relationship, followed by a to-one relationship")]
    [InlineData("[MA]", "a to-many relationship or an attribute")]
    [InlineData("M*", "zero or more to-many relationships")]
    [InlineData("M*O", "zero or more to-many relationships, followed by a to-one relationship")]
    [InlineData("R*M", "zero or more relationships, followed by a to-many relationship")]
    [InlineData("MO+", "a to-many relationship, followed by one or more to-one relationships")]
    [InlineData("[OA]?A", "an optional to-one relationship or attribute, followed by an attribute")]
    [InlineData("O*[MA]", "zero or more to-one relationships, followed by a to-many relationship or an attribute")]
    [InlineData("O?M+A", "an optional to-one relationship, followed by one or more to-many relationships, followed by an attribute")]
    public void ParseSucceeds(string patternText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(patternText);
        pattern.GetDescription().Should().Be(displayText);
    }

    [Theory]
    [InlineData("^", "Pattern is empty.")]
    [InlineData("^X", "Unknown token 'X'.")]
    [InlineData("A^X", "Unknown token 'X'.")]
    [InlineData("^*", "Field type or [ expected.")]
    [InlineData("A+^+", "Field type or [ expected.")]
    [InlineData("[^", "Field type expected.")]
    [InlineData("[^?", "Field type expected.")]
    [InlineData("[^]", "Field type expected.")]
    [InlineData("[A^", "Field type or ] expected.")]
    [InlineData("[MA^", "Field type or ] expected.")]
    [InlineData("[MA^+", "Field type or ] expected.")]
    public void ParseFails(string patternText, string errorMessage)
    {
        // Arrange
        var patternSource = new MarkedText(patternText, '^');

        // Act
        Action action = () => FieldChainPattern.Parse(patternSource.Text);

        // Assert
        PatternFormatException exception = action.Should().Throw<PatternFormatException>().Which;
        exception.Message.Should().Be(errorMessage);
        exception.Position.Should().Be(patternSource.Position);
        exception.Pattern.Should().Be(patternSource.Text);
    }

    [Theory]
    [InlineData("[A]", "A", "an attribute")]
    [InlineData("[MO]", "R", "a relationship")]
    [InlineData("[MOR]", "R", "a relationship")]
    [InlineData("[MORAF]", "F", "a field")]
    [InlineData("[AMAM]", "[MA]", "a to-many relationship or an attribute")]
    public void ParseNormalizesChoices(string patternText, string reducedText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(reducedText);
        pattern.GetDescription().Should().Be(displayText);
    }
}
