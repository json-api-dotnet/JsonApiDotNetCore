using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class HasManyAttributeTests
{
    [Fact]
    public void Cannot_set_value_to_null()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Cannot_set_value_to_primitive_type()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, 1);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' must be a collection.");
    }

    [Fact]
    public void Cannot_set_value_to_single_resource()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, resource);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage($"Resource of type '{typeof(TestResource).FullName}' must be a collection.");
    }

    [Fact]
    public void Can_set_value_to_collection_with_single_resource()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        var children = new List<TestResource>
        {
            resource
        };

        // Act
        attribute.SetValue(resource, children);

        // Assert
        attribute.GetValue(resource).Should().BeOfType<List<TestResource>>().Subject.ShouldHaveCount(1);
    }

    [Fact]
    public void Cannot_set_value_to_collection_with_null_element()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        var children = new List<TestResource>
        {
            resource,
            null!
        };

        // Act
        Action action = () => attribute.SetValue(resource, children);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource collection must not contain null values.");
    }

    [Fact]
    public void Cannot_set_value_to_collection_with_primitive_element()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource();

        var children = new List<object>
        {
            resource,
            1
        };

        // Act
        Action action = () => attribute.SetValue(resource, children);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' does not implement IIdentifiable.");
    }

    private sealed class TestResource : Identifiable<long>
    {
        [HasMany]
        public IEnumerable<TestResource> Children { get; set; } = new HashSet<TestResource>();
    }
}
