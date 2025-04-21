using DapperExample.TranslationToSql.TreeNodes;
using FluentAssertions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class ParameterNodeTests
{
    [Fact]
    public void Throws_on_invalid_name()
    {
        // Act
        Action action = () => _ = new ParameterNode("p1", null);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("Parameter name must start with an '@' symbol and not be empty.*");
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData(-123, "-123")]
    [InlineData(123U, "123")]
    [InlineData(-123L, "-123")]
    [InlineData(123UL, "123")]
    [InlineData((short)-123, "-123")]
    [InlineData((ushort)123, "123")]
    [InlineData('A', "'A'")]
    [InlineData((sbyte)123, "123")]
    [InlineData((byte)123, "0x7B")]
    [InlineData(1.23F, "1.23")]
    [InlineData(1.23D, "1.23")]
    [InlineData("123", "'123'")]
    [InlineData(DayOfWeek.Saturday, "DayOfWeek.Saturday")]
    public void Can_format_parameter(object? parameterValue, string formattedValueExpected)
    {
        // Arrange
        var parameter = new ParameterNode("@name", parameterValue);

        // Act
        string text = parameter.ToString();

        // Assert
        text.Should().Be($"@name = {formattedValueExpected}");
    }
}
