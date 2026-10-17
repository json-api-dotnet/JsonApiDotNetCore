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
    [InlineData("C", "a compound attribute")]
    [InlineData("P", "a primitive collection")]
    [InlineData("Q", "a compound collection")]
    [InlineData("[OC]", "a to-one relationship or a compound attribute")]
    [InlineData("[MPQ]", "a to-many relationship or a primitive collection or a compound collection")]
    [InlineData("[OC]*A", "zero or more to-one relationships or compound attributes, followed by an attribute")]
    [InlineData("[OC]*[MPQ]", "zero or more to-one relationships or compound attributes, followed by a to-many relationship or a primitive collection or a compound collection")]
    [InlineData("[OC]*[OAC]", "zero or more to-one relationships or compound attributes, followed by a to-one relationship or an attribute or a compound attribute")]
    [InlineData("C*A", "zero or more compound attributes, followed by an attribute")]
    [InlineData("C+A", "one or more compound attributes, followed by an attribute")]
    [InlineData("[OC]?", "an optional to-one relationship or compound attribute")]
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
        PatternFormatException exception = action.Should().ThrowExactly<PatternFormatException>().Which;
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
    [InlineData("[C]", "C", "a compound attribute")]
    [InlineData("[P]", "P", "a primitive collection")]
    [InlineData("[Q]", "Q", "a compound collection")]
    [InlineData("[OCOC]", "[OC]", "a to-one relationship or a compound attribute")]
    [InlineData("[QPMP]", "[MPQ]", "a to-many relationship or a primitive collection or a compound collection")]
    [InlineData("[MORACPQ]", "F", "a field")]
    public void ParseNormalizesChoices(string patternText, string reducedText, string displayText)
    {
        // Act
        FieldChainPattern pattern = FieldChainPattern.Parse(patternText);

        // Assert
        pattern.ToString().Should().Be(reducedText);
        pattern.GetDescription().Should().Be(displayText);
    }
}
