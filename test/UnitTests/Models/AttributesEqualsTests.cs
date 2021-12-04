using FluentAssertions;
using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.Models;

public sealed class AttributesEqualsTests
{
    [Fact]
    public void HasManyAttribute_Equals_Returns_True_When_Same_Name()
    {
        // Arrange
        var attribute1 = new HasManyAttribute
        {
            PublicName = "test"
        };

        var attribute2 = new HasManyAttribute
        {
            PublicName = "test"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasManyAttribute_Equals_Returns_False_When_Different_Name()
    {
        // Arrange
        var attribute1 = new HasManyAttribute
        {
            PublicName = "test1"
        };

        var attribute2 = new HasManyAttribute
        {
            PublicName = "test2"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasOneAttribute_Equals_Returns_True_When_Same_Name()
    {
        // Arrange
        var attribute1 = new HasOneAttribute
        {
            PublicName = "test"
        };

        var attribute2 = new HasOneAttribute
        {
            PublicName = "test"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasOneAttribute_Equals_Returns_False_When_Different_Name()
    {
        // Arrange
        var attribute1 = new HasOneAttribute
        {
            PublicName = "test1"
        };

        var attribute2 = new HasOneAttribute
        {
            PublicName = "test2"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AttrAttribute_Equals_Returns_True_When_Same_Name()
    {
        // Arrange
        var attribute1 = new AttrAttribute
        {
            PublicName = "test"
        };

        var attribute2 = new AttrAttribute
        {
            PublicName = "test"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AttrAttribute_Equals_Returns_False_When_Different_Name()
    {
        // Arrange
        var attribute1 = new AttrAttribute
        {
            PublicName = "test1"
        };

        var attribute2 = new AttrAttribute
        {
            PublicName = "test2"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasManyAttribute_Does_Not_Equal_HasOneAttribute_With_Same_Name()
    {
        // Arrange
        RelationshipAttribute attribute1 = new HasManyAttribute
        {
            PublicName = "test"
        };

        RelationshipAttribute attribute2 = new HasOneAttribute
        {
            PublicName = "test"
        };

        // Act
        bool result = attribute1.Equals(attribute2);

        // Assert
        result.Should().BeFalse();
    }
}
