using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class HasOneAttributeTests
{
    [Fact]
    public void Can_set_value_to_null()
    {
        // Arrange
        var attribute = new HasOneAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Parent))!
        };

        var resource = new TestResource();

        // Act
        attribute.SetValue(resource, null);

        // Assert
        attribute.GetValue(resource).Should().BeNull();
    }

    [Fact]
    public void Can_set_value_to_self()
    {
        // Arrange
        var attribute = new HasOneAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Parent))!
        };

        var resource = new TestResource();

        // Act
        attribute.SetValue(resource, resource);

        // Assert
        attribute.GetValue(resource).Should().Be(resource);
    }

    [Fact]
    public void Cannot_set_value_to_primitive_type()
    {
        // Arrange
        var attribute = new HasOneAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Parent))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, 1);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' does not implement IIdentifiable.");
    }

    private sealed class TestResource : Identifiable<long>
    {
        [HasOne]
        public TestResource? Parent { get; set; }
    }
}
