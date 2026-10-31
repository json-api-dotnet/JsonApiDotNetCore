using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class ResourceFieldAttributeTests
{
    [Fact]
    public void Cannot_set_public_name_to_null()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.PublicName = null!;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("Exposed name cannot be null, empty or contain only whitespace. (Parameter 'value')");
    }

    [Fact]
    public void Cannot_set_public_name_to_empty()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.PublicName = string.Empty;

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("Exposed name cannot be null, empty or contain only whitespace. (Parameter 'value')");
    }

    [Fact]
    public void Cannot_set_public_name_to_whitespace()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.PublicName = " ";

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("Exposed name cannot be null, empty or contain only whitespace. (Parameter 'value')");
    }

    [Fact]
    public void Cannot_get_value_for_null()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.GetValue(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Cannot_get_value_for_primitive_type()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.GetValue(1);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' does not implement IIdentifiable.");
    }

    [Fact]
    public void Cannot_get_value_for_write_only_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.WriteOnlyAttribute))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.GetValue(resource);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Property 'TestResource.WriteOnlyAttribute' is write-only.");
    }

    [Fact]
    public void Cannot_get_value_for_unknown_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(IHttpContextAccessor).GetProperty(nameof(IHttpContextAccessor.HttpContext))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.GetValue(resource);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Unable to get property value of 'IHttpContextAccessor.HttpContext' on instance of type 'TestResource'.")
            .WithInnerException<TargetException>();
    }

    [Fact]
    public void Cannot_get_value_for_throwing_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.ThrowOnGetAttribute))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.GetValue(resource);

        // Assert
        action.Should().ThrowExactly<TargetInvocationException>().WithInnerException<NotSupportedException>().WithMessage("Getting value is not supported.");
    }

    [Fact]
    public void Cannot_set_value_for_null()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.SetValue(null!, "some");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Cannot_set_value_for_primitive_type()
    {
        // Arrange
        var attribute = new AttrAttribute();

        // Act
        Action action = () => attribute.SetValue(1, "some");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Resource of type 'System.Int32' does not implement IIdentifiable.");
    }

    [Fact]
    public void Cannot_set_value_for_read_only_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.ReadOnlyAttribute))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, true);

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>().WithMessage("Property 'TestResource.ReadOnlyAttribute' is read-only.");
    }

    [Fact]
    public void Cannot_set_value_for_unknown_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(IHttpContextAccessor).GetProperty(nameof(IHttpContextAccessor.HttpContext))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, "some");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Unable to set property value of 'IHttpContextAccessor.HttpContext' on instance of type 'TestResource'.")
            .WithInnerException<TargetException>();
    }

    [Fact]
    public void Cannot_set_value_for_throwing_resource_property()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.ThrowOnSetAttribute))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, 1);

        // Assert
        action.Should().ThrowExactly<TargetInvocationException>().WithInnerException<NotSupportedException>().WithMessage("Setting value is not supported.");
    }

    [Fact]
    public void Cannot_set_value_to_incompatible_value()
    {
        // Arrange
        var attribute = new AttrAttribute
        {
            Property = typeof(TestResource).GetProperty(nameof(TestResource.WriteOnlyAttribute))!
        };

        var resource = new TestResource();

        // Act
        Action action = () => attribute.SetValue(resource, DateTime.UtcNow);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage("Object of type 'System.DateTime' cannot be converted to type 'System.Boolean'.");
    }

    private sealed class TestResource : Identifiable<long>
    {
        [Attr]
        public bool ReadOnlyAttribute => true;

        [Attr]
        public bool WriteOnlyAttribute
        {
            set => _ = value;
        }

        [Attr]
        public int ThrowOnGetAttribute => throw new NotSupportedException("Getting value is not supported.");

        [Attr]
        public int ThrowOnSetAttribute
        {
            get => 1;
            set => throw new NotSupportedException("Setting value is not supported.");
        }
    }
}
