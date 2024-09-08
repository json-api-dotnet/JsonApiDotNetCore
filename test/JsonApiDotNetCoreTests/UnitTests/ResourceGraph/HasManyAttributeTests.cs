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

        List<TestResource> children = [resource];

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

        List<TestResource> children =
        [
            resource,
            null!
        ];

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

        List<object> children =
        [
            resource,
            1
        ];

        // Act
        Action action = () => attribute.SetValue(resource, children);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' does not implement IIdentifiable.");
    }

    [Fact]
    public void Can_add_value_to_List()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource
        {
            Children = new List<TestResource>
            {
                new()
            }
        };

        var resourceToAdd = new TestResource();

        // Act
        attribute.AddValue(resource, resourceToAdd);

        // Assert
        List<TestResource> collection = attribute.GetValue(resource).Should().BeOfType<List<TestResource>>().Subject!;
        collection.ShouldHaveCount(2);
    }

    [Fact]
    public void Can_add_existing_value_to_List()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resourceToAdd = new TestResource();

        var resource = new TestResource
        {
            Children = new List<TestResource>
            {
                resourceToAdd
            }
        };

        // Act
        attribute.AddValue(resource, resourceToAdd);

        // Assert
        List<TestResource> collection = attribute.GetValue(resource).Should().BeOfType<List<TestResource>>().Subject!;
        collection.ShouldHaveCount(1);
    }

    [Fact]
    public void Can_add_value_to_HashSet()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource
        {
            Children = new HashSet<TestResource>
            {
                new()
            }
        };

        var resourceToAdd = new TestResource();

        // Act
        attribute.AddValue(resource, resourceToAdd);

        // Assert
        HashSet<TestResource> collection = attribute.GetValue(resource).Should().BeOfType<HashSet<TestResource>>().Subject!;
        collection.ShouldHaveCount(2);
    }

    [Fact]
    public void Can_add_existing_value_to_HashSet()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resourceToAdd = new TestResource();

        var resource = new TestResource
        {
            Children = new HashSet<TestResource>
            {
                resourceToAdd
            }
        };

        // Act
        attribute.AddValue(resource, resourceToAdd);

        // Assert
        HashSet<TestResource> collection = attribute.GetValue(resource).Should().BeOfType<HashSet<TestResource>>().Subject!;
        collection.ShouldHaveCount(1);
    }

    [Fact]
    public void Can_add_value_to_null_collection()
    {
        // Arrange
        var attribute = new HasManyAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.Children))!
        };

        var resource = new TestResource
        {
            Children = null!
        };

        var resourceToAdd = new TestResource();

        // Act
        attribute.AddValue(resource, resourceToAdd);

        // Assert
        HashSet<TestResource> collection = attribute.GetValue(resource).Should().BeOfType<HashSet<TestResource>>().Subject!;
        collection.ShouldHaveCount(1);
    }

    public sealed class TestResource : Identifiable<long>
    {
        [HasMany]
        public IEnumerable<TestResource> Children { get; set; } = new HashSet<TestResource>();
    }
}
