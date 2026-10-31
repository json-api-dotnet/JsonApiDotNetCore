using System.Reflection;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class CompoundTypeTests
{
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
        compoundType.ClrType.Should().Be<TestAddress>();
        compoundType.Attributes.Should().ContainSingle().Which.Should().Be(streetAttribute);
        compoundType.ToString().Should().Be(nameof(TestAddress));

        compoundType.FindAttributeByPublicName("street").Should().Be(streetAttribute);
        compoundType.FindAttributeByPublicName("unknown").Should().BeNull();
        compoundType.GetAttributeByPublicName("street").Should().Be(streetAttribute);
        Action getUnknownPublic = () => compoundType.GetAttributeByPublicName("unknown");
        getUnknownPublic.Should().ThrowExactly<InvalidOperationException>().WithMessage("Attribute 'unknown' does not exist on compound type 'TestAddress'.");

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
        var compoundType = new CompoundType(typeof(TestAddress), []);

        var resourceGraph = new JsonApiDotNetCore.Configuration.ResourceGraph(new HashSet<ResourceType>().AsReadOnly(), new HashSet<CompoundType>
        {
            compoundType
        }.AsReadOnly());

        // Act & Assert
        resourceGraph.GetCompoundTypes().Should().ContainSingle().Which.Should().Be(compoundType);
        resourceGraph.FindCompoundType<TestAddress>().Should().Be(compoundType);
        resourceGraph.FindCompoundType<string>().Should().BeNull();

        resourceGraph.GetCompoundType<TestAddress>().Should().Be(compoundType);
        Action getUnknown = () => resourceGraph.GetCompoundType<string>();
        getUnknown.Should().ThrowExactly<InvalidOperationException>().WithMessage("Compound type 'System.String' does not exist in the resource graph.");
    }

    [Fact]
    public void Finds_attributes_in_built_compound_type_by_public_name_and_property_name()
    {
        // Arrange
        IResourceGraph resourceGraph = BuildGraph();

        // Act
        CompoundType compoundType = resourceGraph.GetCompoundType<ShippingAddress>();

        // Assert
        AttrAttribute byPublicName = compoundType.GetAttributeByPublicName("zip-code");
        AttrAttribute byPropertyName = compoundType.GetAttributeByPropertyName(nameof(ShippingAddress.ZipCode));

        byPublicName.Should().BeSameAs(byPropertyName);
        compoundType.FindAttributeByPublicName(nameof(ShippingAddress.ZipCode)).Should().BeNull();
        compoundType.FindAttributeByPropertyName("zip-code").Should().BeNull();
        compoundType.FindAttributeByPublicName("zip-code").Should().BeSameAs(byPublicName);
    }

    [Fact]
    public void Equality_is_structural()
    {
        // Arrange
        IResourceGraph first = BuildGraph();
        IResourceGraph second = BuildGraph();

        // Act
        CompoundType firstCompound = first.GetCompoundType<ShippingAddress>();
        CompoundType secondCompound = second.GetCompoundType<ShippingAddress>();

        AttrAttribute firstAttribute = firstCompound.GetAttributeByPropertyName(nameof(ShippingAddress.Street));
        AttrAttribute secondAttribute = secondCompound.GetAttributeByPropertyName(nameof(ShippingAddress.Street));

        // Assert
        firstCompound.Should().NotBeSameAs(secondCompound);
        firstCompound.Equals(secondCompound).Should().BeTrue();
        firstCompound.GetHashCode().Should().Be(secondCompound.GetHashCode());

        firstAttribute.Should().NotBeSameAs(secondAttribute);
        firstAttribute.Equals(secondAttribute).Should().BeTrue();
        firstAttribute.GetHashCode().Should().Be(secondAttribute.GetHashCode());

        firstCompound.Equals(first.GetCompoundType<Tag>()).Should().BeFalse();
        firstCompound.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equality_of_compound_attributes_is_structural()
    {
        // Arrange
        IResourceGraph first = BuildGraph();
        IResourceGraph second = BuildGraph();

        // Act
        AttrAttribute firstAttribute = first.GetResourceType<Customer>().GetAttributeByPropertyName(nameof(Customer.Address));
        AttrAttribute secondAttribute = second.GetResourceType<Customer>().GetAttributeByPropertyName(nameof(Customer.Address));

        // Assert
        firstAttribute.Equals(secondAttribute).Should().BeTrue();
        firstAttribute.GetHashCode().Should().Be(secondAttribute.GetHashCode());
    }

    [Fact]
    public void ToString_distinguishes_generic_arguments()
    {
        // Arrange
        IResourceGraph resourceGraph = BuildGraph();

        // Act
        string text = resourceGraph.GetCompoundType<Labeled<string>>().ToString();
        string date = resourceGraph.GetCompoundType<Labeled<DateOnly>>().ToString();

        // Assert
        text.Should().NotBe(date);
    }

    [Fact]
    public void ResourceGraph_Get_and_Find_overloads()
    {
        // Arrange
        IResourceGraph resourceGraph = BuildGraph();

        Type customerClrType = typeof(Customer);
        Type addressClrType = typeof(ShippingAddress);

        // Act & Assert
        resourceGraph.GetResourceType(customerClrType).ClrType.Should().Be<Customer>();
        resourceGraph.GetResourceType<Customer>().ClrType.Should().Be<Customer>();
        resourceGraph.FindResourceType(customerClrType).Should().NotBeNull();
        resourceGraph.FindResourceType<Customer>().Should().NotBeNull();
        resourceGraph.FindResourceType(addressClrType).Should().BeNull();

        resourceGraph.GetCompoundType(addressClrType).ClrType.Should().Be<ShippingAddress>();
        resourceGraph.GetCompoundType<ShippingAddress>().ClrType.Should().Be<ShippingAddress>();
        resourceGraph.FindCompoundType(addressClrType).Should().NotBeNull();
        resourceGraph.FindCompoundType<ShippingAddress>().Should().NotBeNull();
        resourceGraph.FindCompoundType(customerClrType).Should().BeNull();
        resourceGraph.FindCompoundType<Customer>().Should().BeNull();

        Action getResourceType = () => resourceGraph.GetResourceType(addressClrType);
        getResourceType.Should().ThrowExactly<InvalidOperationException>();

        Action getCompoundType = () => resourceGraph.GetCompoundType(customerClrType);

        getCompoundType.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage($"Compound type '{typeof(Customer)}' does not exist in the resource graph.");

        Action getCompoundTypeGeneric = () => resourceGraph.GetCompoundType<Customer>();
        getCompoundTypeGeneric.Should().ThrowExactly<InvalidOperationException>();

        Action findCompoundTypeNull = () => resourceGraph.FindCompoundType(null!);
        findCompoundTypeNull.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void AttrAttribute_Equals_includes_compound_metadata()
    {
        // Arrange
        PropertyInfo property = typeof(TestAddress).GetProperty(nameof(TestAddress.Street))!;
        var compoundType = new CompoundType(typeof(TestAddress), []);
        var otherCompoundType = new CompoundType(typeof(Tag), []);

        AttrAttribute CreateAttribute(bool isCompound, bool isCollection, CompoundType? type)
        {
            return new AttrAttribute
            {
                PublicName = "street",
                Property = property,
                IsCompound = isCompound,
                IsCollection = isCollection,
                CompoundType = type
            };
        }

        AttrAttribute baseline = CreateAttribute(true, false, compoundType);

        // Act & Assert
        baseline.Equals(CreateAttribute(true, false, compoundType)).Should().BeTrue();
        baseline.GetHashCode().Should().Be(CreateAttribute(true, false, compoundType).GetHashCode());

        baseline.Equals(CreateAttribute(false, false, compoundType)).Should().BeFalse();
        baseline.Equals(CreateAttribute(true, true, compoundType)).Should().BeFalse();
        baseline.Equals(CreateAttribute(true, false, otherCompoundType)).Should().BeFalse();
        baseline.Equals(CreateAttribute(true, false, null)).Should().BeFalse();
    }

    private static IResourceGraph BuildGraph()
    {
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<Customer, long>();
        return builder.Build();
    }

    private sealed class TestAddress
    {
        public string? Street { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ShippingAddress
    {
        [Attr]
        public string? Street { get; set; }

        [Attr(PublicName = "zip-code")]
        public string? ZipCode { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Labeled<T>
    {
        [Attr]
        public T? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Tag
    {
        [Attr]
        public string? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Customer : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ShippingAddress? Address { get; set; }

        [Attr(IsCompound = true)]
        public Labeled<string>? Text { get; set; }

        [Attr(IsCompound = true)]
        public Labeled<DateOnly>? Date { get; set; }

        [Attr(IsCompound = true)]
        public List<Tag> Tags { get; set; } = [];
    }
}
