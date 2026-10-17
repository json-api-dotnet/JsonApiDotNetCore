using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class CompoundTypeTests
{
    [Fact]
    public void Cannot_create_compound_type_without_clr_type()
    {
        // Act
        Action action = () => _ = new CompoundType(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Can_lookup_attributes_in_compound_type()
    {
        // Arrange
        PropertyInfo streetProperty = typeof(TestAddress).GetProperty(nameof(TestAddress.Street))!;
        var streetAttribute = new AttrAttribute
        {
            PublicName = "street",
            Property = streetProperty
        };

        var compoundType = new CompoundType(typeof(TestAddress), [streetAttribute]);

        // Act & Assert
        compoundType.ClrType.Should().Be(typeof(TestAddress));
        compoundType.Attributes.Should().ContainSingle().Which.Should().Be(streetAttribute);
        compoundType.ToString().Should().Be(nameof(TestAddress));

        compoundType.FindAttributeByPublicName("street").Should().Be(streetAttribute);
        compoundType.FindAttributeByPublicName("unknown").Should().BeNull();
        compoundType.GetAttributeByPublicName("street").Should().Be(streetAttribute);
        Action getUnknownPublic = () => compoundType.GetAttributeByPublicName("unknown");
        getUnknownPublic.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Attribute 'unknown' does not exist on compound type 'TestAddress'.");

        compoundType.FindAttributeByPropertyName(nameof(TestAddress.Street)).Should().Be(streetAttribute);
        compoundType.FindAttributeByPropertyName("Unknown").Should().BeNull();
        compoundType.GetAttributeByPropertyName(nameof(TestAddress.Street)).Should().Be(streetAttribute);
        Action getUnknownProp = () => compoundType.GetAttributeByPropertyName("Unknown");
        getUnknownProp.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Attribute for property 'Unknown' does not exist on compound type 'TestAddress'.");
    }

    [Fact]
    public void Can_lookup_compound_type_in_resource_graph()
    {
        // Arrange
        var compoundType = new CompoundType(typeof(TestAddress));
        var resourceGraph = new JsonApiDotNetCore.Configuration.ResourceGraph(
            new HashSet<ResourceType>().AsReadOnly(),
            new HashSet<CompoundType> { compoundType }.AsReadOnly());

        // Act & Assert
        resourceGraph.GetCompoundTypes().Should().ContainSingle().Which.Should().Be(compoundType);
        resourceGraph.FindCompoundType(typeof(TestAddress)).Should().Be(compoundType);
        resourceGraph.FindCompoundType(typeof(string)).Should().BeNull();

        resourceGraph.GetCompoundType(typeof(TestAddress)).Should().Be(compoundType);
        Action getUnknown = () => resourceGraph.GetCompoundType(typeof(string));
        getUnknown.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Compound type 'System.String' does not exist in the resource graph.");
    }

    private sealed class TestAddress
    {
        public string? Street { get; set; }
    }
}
