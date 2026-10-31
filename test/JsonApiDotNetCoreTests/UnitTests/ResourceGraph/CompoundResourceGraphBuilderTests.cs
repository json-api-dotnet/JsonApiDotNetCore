using System.Linq.Expressions;
using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class CompoundResourceGraphBuilderTests
{
    public static TheoryData<Expression<Func<ChainResource, object?>>, string> ValidFieldChains =>
        new()
        {
            { resource => resource.Compound!.Child!.Scalar, "compound.child.scalar" },
            { resource => resource.Related!.Compound!.Number, "related.compound.number" },
            { resource => resource.Compound!.Number, "compound.number" },
            { resource => resource.Compound!.Children, "compound.children" },
            { resource => resource.Id, "id" }
        };

    public static TheoryData<Expression<Func<ChainResource, object?>>, string> InvalidFieldChains =>
        new()
        {
            {
                resource => new
                {
                    resource.Compound!.Number,
                    resource.Compound.Text
                },
                "must select a single property or chain"
            },
            { resource => resource.Compound!.Number.ToString(), "should select a single property or chain, or select multiple properties" },
            {
                resource => new
                {
                    Value = resource.Compound!.Number.ToString()
                },
                "contains an invalid member selection. Expected member access expression"
            },
            { resource => resource.Compound!.Children![0].Scalar, "contains an invalid expression" },
            { resource => resource.NotExposed, "is not exposed as a JSON:API field on" },
            { resource => resource.Compound!.Text!.Length, "cannot have nested fields because it is neither a compound attribute nor a relationship" }
        };

    [Fact]
    public void Shares_one_CompoundType_across_attributes_and_resources()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<FirstSharingResource, long>();
        builder.Add<SecondSharingResource, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType first = resourceGraph.GetResourceType<FirstSharingResource>().GetAttributeByPropertyName(nameof(FirstSharingResource.Home)).CompoundType!;
        CompoundType second = resourceGraph.GetResourceType<FirstSharingResource>().GetAttributeByPropertyName(nameof(FirstSharingResource.Work)).CompoundType!;

        CompoundType third =
            resourceGraph.GetResourceType<SecondSharingResource>().GetAttributeByPropertyName(nameof(SecondSharingResource.Other)).CompoundType!;

        second.Should().BeSameAs(first);
        third.Should().BeSameAs(first);
        resourceGraph.GetCompoundTypes().Should().ContainSingle();
    }

    [Fact]
    public void Distinguishes_closed_generic_compound_types()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithGenericCompounds, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithGenericCompounds>();
        CompoundType firstText = resourceType.GetAttributeByPropertyName(nameof(ResourceWithGenericCompounds.FirstText)).CompoundType!;
        CompoundType secondText = resourceType.GetAttributeByPropertyName(nameof(ResourceWithGenericCompounds.SecondText)).CompoundType!;
        CompoundType date = resourceType.GetAttributeByPropertyName(nameof(ResourceWithGenericCompounds.Date)).CompoundType!;

        secondText.Should().BeSameAs(firstText);
        date.Should().NotBeSameAs(firstText);
        firstText.ClrType.Should().Be<LabeledValue<string>>();
        date.ClrType.Should().Be<LabeledValue<DateOnly>>();
        resourceGraph.GetCompoundTypes().Should().HaveCount(2);
    }

    [Fact]
    public void Sets_ContainingType_on_nested_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithNestedCompounds, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithNestedCompounds>();
        resourceType.Attributes.Should().OnlyContain(attribute => attribute.ContainingType == resourceType);

        foreach (CompoundType compoundType in resourceGraph.GetCompoundTypes())
        {
            compoundType.Attributes.Should().OnlyContain(attribute => attribute.ContainingType == compoundType);
        }

        AttrAttribute outer = resourceType.GetAttributeByPropertyName(nameof(ResourceWithNestedCompounds.Outer));
        AttrAttribute inner = outer.CompoundType!.GetAttributeByPropertyName(nameof(OuterCompound.Inner));
        AttrAttribute leaf = inner.CompoundType!.GetAttributeByPropertyName(nameof(InnerCompound.Leaf));

        outer.ContainingType.Should().BeSameAs(resourceType);
        inner.ContainingType.Should().BeSameAs(outer.CompoundType);
        leaf.ContainingType.Should().BeSameAs(inner.CompoundType);
    }

    [Theory]
    [InlineData(nameof(ResourceWithCollectionKinds.List), true)]
    [InlineData(nameof(ResourceWithCollectionKinds.Array), true)]
    [InlineData(nameof(ResourceWithCollectionKinds.Set), true)]
    [InlineData(nameof(ResourceWithCollectionKinds.Enumerable), true)]
    [InlineData(nameof(ResourceWithCollectionKinds.Text), false)]
    [InlineData(nameof(ResourceWithCollectionKinds.Number), false)]
    public void Classifies_IsCollection(string propertyName, bool expected)
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCollectionKinds, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        AttrAttribute attribute = resourceGraph.GetResourceType<ResourceWithCollectionKinds>().GetAttributeByPropertyName(propertyName);
        attribute.IsCollection.Should().Be(expected);
    }

    [Fact]
    public void Classifies_byte_array_as_collection()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCollectionKinds, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        AttrAttribute attribute = resourceGraph.GetResourceType<ResourceWithCollectionKinds>()
            .GetAttributeByPropertyName(nameof(ResourceWithCollectionKinds.Bytes));

        attribute.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Does_not_classify_dictionary_as_collection()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCollectionKinds, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        AttrAttribute attribute = resourceGraph.GetResourceType<ResourceWithCollectionKinds>()
            .GetAttributeByPropertyName(nameof(ResourceWithCollectionKinds.Map));

        attribute.IsCollection.Should().BeFalse();
    }

    [Fact]
    public void Unwraps_collection_element_type_for_compound_discovery()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCompoundCollections, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithCompoundCollections>();
        AttrAttribute listAttribute = resourceType.GetAttributeByPropertyName(nameof(ResourceWithCompoundCollections.AsList));
        AttrAttribute arrayAttribute = resourceType.GetAttributeByPropertyName(nameof(ResourceWithCompoundCollections.AsArray));

        listAttribute.CompoundType!.ClrType.Should().Be<InnerCompound>();
        arrayAttribute.CompoundType!.ClrType.Should().Be<InnerCompound>();
        arrayAttribute.CompoundType.Should().BeSameAs(listAttribute.CompoundType);
        resourceGraph.GetCompoundTypes().Should().ContainSingle();
    }

    [Fact]
    public void Unwraps_nullable_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithNullableCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        AttrAttribute attribute = resourceGraph.GetResourceType<ResourceWithNullableCompound>()
            .GetAttributeByPropertyName(nameof(ResourceWithNullableCompound.Compound));

        attribute.CompoundType!.ClrType.Should().Be<InnerCompound>();
    }

    [Fact]
    public void Strips_AllowSort_from_nested_compound_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithSortableNestedCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType outer = resourceGraph.GetCompoundType<SortableOuterCompound>();
        AttrAttribute nested = outer.GetAttributeByPropertyName(nameof(SortableOuterCompound.Nested));

        nested.Capabilities.Should().NotHaveFlag(AttrCapabilities.AllowSort);
        nested.Capabilities.Should().HaveFlag(AttrCapabilities.AllowFilter);

        AttrAttribute scalar = outer.GetAttributeByPropertyName(nameof(SortableOuterCompound.Scalar));
        scalar.Capabilities.Should().HaveFlag(AttrCapabilities.AllowSort);
    }

    [Fact]
    public void Applies_default_capabilities_to_nested_attributes()
    {
        // Arrange
        var options = new JsonApiOptions
        {
            DefaultAttrCapabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowFilter
        };

        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCapabilitiesInCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType compoundType = resourceGraph.GetCompoundType<CompoundWithCapabilities>();
        AttrAttribute attribute = compoundType.GetAttributeByPropertyName(nameof(CompoundWithCapabilities.Default));

        attribute.Capabilities.Should().Be(AttrCapabilities.AllowView | AttrCapabilities.AllowFilter);
    }

    [Fact]
    public void Preserves_explicit_capabilities_on_nested_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCapabilitiesInCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType compoundType = resourceGraph.GetCompoundType<CompoundWithCapabilities>();
        AttrAttribute attribute = compoundType.GetAttributeByPropertyName(nameof(CompoundWithCapabilities.Explicit));

        attribute.Capabilities.Should().Be(AttrCapabilities.All & ~AttrCapabilities.AllowView);
    }

    [Theory]
    [InlineData(typeof(ResourceWithIdentifiableCompound))]
    [InlineData(typeof(ResourceWithIdentifiableCompoundCollection))]
    [InlineData(typeof(ResourceWithNullableIdentifiableCompound))]
    public void Cannot_use_identifiable_type_as_compound(Type resourceClrType)
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add(resourceClrType, typeof(long));

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{resourceClrType.Name}.Child' implements IIdentifiable and must be exposed as a JSON:API relationship using [HasOne] or [HasMany], not as a compound attribute.");
    }

    [Theory]
    [InlineData(typeof(ResourceWithSelfCollectionCycle), typeof(SelfCollectionCycleCompound))]
    [InlineData(typeof(ResourceWithIndirectCollectionCycle), typeof(IndirectCollectionCycleCompoundA))]
    public void Detects_circular_references_in_compound_types_via_collection(Type resourceClrType, Type expectedCompoundClrType)
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add(resourceClrType, typeof(long));

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Circular reference detected: compound type '{expectedCompoundClrType}' references itself directly or indirectly.");
    }

    [Fact]
    public void Rejects_interface_as_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithInterfaceCompound, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"*{nameof(IInterfaceCompound)}*");
    }

    [Theory]
    [InlineData(typeof(ResourceWithStructCompound))]
    [InlineData(typeof(ResourceWithNullableStructCompound))]
    public void Accepts_struct_and_nullable_struct_compound_type(Type resourceClrType)
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add(resourceClrType, typeof(long));

        // Act
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType compoundType = resourceGraph.GetCompoundType<GeoPoint>();
        compoundType.ClrType.Should().Be<GeoPoint>();
        resourceGraph.FindCompoundType<GeoPoint?>().Should().BeSameAs(compoundType);
    }

    [Fact]
    public void Rejects_abstract_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithAbstractCompound, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"*{nameof(AbstractCompound)}*");
    }

    [Fact]
    public void Rejects_compound_type_without_parameterless_constructor()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithPositionalCompound, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"*{nameof(PositionalCompound)}*parameterless constructor*");
    }

    [Theory]
    [InlineData(typeof(ResourceWithStringCompound))]
    [InlineData(typeof(ResourceWithInt32Compound))]
    [InlineData(typeof(ResourceWithDateTimeCompound))]
    [InlineData(typeof(ResourceWithGuidCompound))]
    public void Rejects_IsCompound_on_scalar_types(Type resourceClrType)
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add(resourceClrType, typeof(long));

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>();
    }

    [Fact]
    public void Logs_warning_when_compound_type_has_no_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add<ResourceWithEmptyCompound, long>();
        builder.Build();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().ContainSingle().Which.Should().Be($"[WARNING] Type '{typeof(EmptyCompound)}' does not contain any attributes.");
    }

    [Fact]
    public void Cannot_build_graph_when_derived_type_redeclares_compound_as_scalar()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<BaseWithCompound, long>();

        // Act
        Action action = () => builder.Add<DerivedRedeclaringCompoundAsScalar, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>();
    }

    [Fact]
    public void Cannot_build_graph_when_derived_type_redeclares_compound_with_different_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<BaseWithCompound, long>();

        // Act
        Action action = () => builder.Add<DerivedRedeclaringCompoundWithOtherType, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>();
    }

    [Fact]
    public void Cannot_build_graph_when_derived_type_redeclares_compound_as_collection()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<BaseWithCompound, long>();

        // Act
        Action action = () => builder.Add<DerivedRedeclaringCompoundAsCollection, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>();
    }

    [Fact]
    public void Cannot_use_duplicate_attribute_name_in_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithDuplicateNameInCompound, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Properties '{typeof(CompoundWithDuplicateName)}.First' and '{typeof(CompoundWithDuplicateName)}.Second' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_use_duplicate_attribute_name_in_compound_type_base_class()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithDuplicateNameInDerivedCompound, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("*both use public name 'duplicate'.");
    }

    [Fact]
    public void Logs_no_warning_for_compound_types_without_IIdentifiable()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add<FirstSharingResource, long>();
        builder.Add(typeof(AddressCompound));
        builder.Build();

        // Assert
        loggerProvider.GetLines().Should().BeEmpty();
    }

    [Fact]
    public void Applies_naming_convention_to_nested_attributes()
    {
        // Arrange
        var options = new JsonApiOptions
        {
            SerializerOptions =
            {
                PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
            }
        };

        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithNamedNestedAttributes, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        CompoundType compoundType = resourceGraph.GetCompoundType<NamedNestedCompound>();

        compoundType.GetAttributeByPropertyName(nameof(NamedNestedCompound.FirstName)).PublicName.Should().Be("first-name");
        compoundType.GetAttributeByPropertyName(nameof(NamedNestedCompound.ZipCode)).PublicName.Should().Be("ZIP_Code");
    }

    [Fact]
    public void Inherits_compound_attributes_from_base_resource()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<BaseWithInheritedCompound, long>();
        builder.Add<DerivedWithInheritedCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        AttrAttribute baseAttribute = resourceGraph.GetResourceType<BaseWithInheritedCompound>()
            .GetAttributeByPropertyName(nameof(BaseWithInheritedCompound.Compound));

        AttrAttribute derivedAttribute = resourceGraph.GetResourceType<DerivedWithInheritedCompound>()
            .GetAttributeByPropertyName(nameof(BaseWithInheritedCompound.Compound));

        derivedAttribute.Should().NotBeSameAs(baseAttribute);
        derivedAttribute.CompoundType.Should().BeSameAs(baseAttribute.CompoundType);
        derivedAttribute.ContainingType.Should().BeSameAs(resourceGraph.GetResourceType<DerivedWithInheritedCompound>());
    }

    [Theory]
    [MemberData(nameof(ValidFieldChains))]
    public void GetFieldChain_resolves_chains(Expression<Func<ChainResource, object?>> selector, string expectedChain)
    {
        // Arrange
        IResourceGraph resourceGraph = CreateChainResourceGraph();

        // Act
        ResourceFieldChainExpression chain = resourceGraph.GetFieldChain(selector);

        // Assert
        chain.ToString().Should().Be(expectedChain);
    }

    [Theory]
    [MemberData(nameof(InvalidFieldChains))]
    public void GetFieldChain_rejects_invalid_expressions(Expression<Func<ChainResource, object?>> selector, string expectedMessagePart)
    {
        // Arrange
        IResourceGraph resourceGraph = CreateChainResourceGraph();

        // Act
        Action action = () => resourceGraph.GetFieldChain(selector);

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithMessage($"*{expectedMessagePart}*");
    }

    private static IResourceGraph CreateChainResourceGraph()
    {
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ChainResource, long>();
        return builder.Build();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AddressCompound
    {
        [Attr]
        public string? Street { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class FirstSharingResource : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public AddressCompound? Home { get; set; }

        [Attr(IsCompound = true)]
        public AddressCompound? Work { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class SecondSharingResource : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public AddressCompound? Other { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class LabeledValue<T>
    {
        [Attr]
        public string? Label { get; set; }

        [Attr]
        public T? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithGenericCompounds : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public LabeledValue<string>? FirstText { get; set; }

        [Attr(IsCompound = true)]
        public LabeledValue<string>? SecondText { get; set; }

        [Attr(IsCompound = true)]
        public LabeledValue<DateOnly>? Date { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class InnerCompound
    {
        [Attr]
        public string? Leaf { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class OuterCompound
    {
        [Attr(IsCompound = true)]
        public InnerCompound? Inner { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithNestedCompounds : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public OuterCompound? Outer { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithCollectionKinds : Identifiable<long>
    {
        [Attr]
        public List<int>? List { get; set; }

        [Attr]
        public int[]? Array { get; set; }

        [Attr]
        public ISet<int>? Set { get; set; }

        [Attr]
        public IEnumerable<int>? Enumerable { get; set; }

        [Attr]
        public string? Text { get; set; }

        [Attr]
        public int Number { get; set; }

        [Attr]
        public byte[]? Bytes { get; set; }

        [Attr]
        public Dictionary<string, string>? Map { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithCompoundCollections : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public List<InnerCompound>? AsList { get; set; }

        [Attr(IsCompound = true)]
        public InnerCompound[]? AsArray { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithNullableCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public InnerCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class SortableOuterCompound
    {
        [Attr(IsCompound = true, Capabilities = AttrCapabilities.All)]
        public InnerCompound? Nested { get; set; }

        [Attr(Capabilities = AttrCapabilities.All)]
        public string? Scalar { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithSortableNestedCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public SortableOuterCompound? Outer { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundWithCapabilities
    {
        [Attr]
        public string? Default { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
        public string? Explicit { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithCapabilitiesInCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithCapabilities? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class IdentifiableChild : Identifiable<long>;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithIdentifiableCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public IdentifiableChild Child { get; set; } = null!;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithIdentifiableCompoundCollection : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public List<IdentifiableChild> Child { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithNullableIdentifiableCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public IdentifiableChild? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class SelfCollectionCycleCompound
    {
        [Attr(IsCompound = true)]
        public List<SelfCollectionCycleCompound>? Children { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithSelfCollectionCycle : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public SelfCollectionCycleCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class IndirectCollectionCycleCompoundA
    {
        [Attr(IsCompound = true)]
        public List<IndirectCollectionCycleCompoundB>? Items { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class IndirectCollectionCycleCompoundB
    {
        [Attr(IsCompound = true)]
        public IndirectCollectionCycleCompoundA? Parent { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithIndirectCollectionCycle : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public IndirectCollectionCycleCompoundA? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface IInterfaceCompound
    {
        [Attr]
        string? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithInterfaceCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public IInterfaceCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public struct GeoPoint
    {
        [Attr]
        public double Latitude { get; set; }

        [Attr]
        public double Longitude { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithStructCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public GeoPoint Location { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithNullableStructCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public GeoPoint? Location { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class AbstractCompound
    {
        [Attr]
        public string? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithAbstractCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public AbstractCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed record PositionalCompound([property: Attr] string? Name);

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithPositionalCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public PositionalCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithStringCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public string? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithInt32Compound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public int Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDateTimeCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public DateTime Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithGuidCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public Guid Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class EmptyCompound
    {
        public string? NotExposed { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithEmptyCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public EmptyCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class BaseWithCompound : Identifiable<long>
    {
        [Attr(PublicName = "shape", IsCompound = true)]
        public InnerCompound? Shape { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedRedeclaringCompoundAsScalar : BaseWithCompound
    {
        [Attr(PublicName = "shape")]
        public new string? Shape { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedRedeclaringCompoundWithOtherType : BaseWithCompound
    {
        [Attr(PublicName = "shape", IsCompound = true)]
        public new AddressCompound? Shape { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedRedeclaringCompoundAsCollection : BaseWithCompound
    {
        [Attr(PublicName = "shape", IsCompound = true)]
        public new List<InnerCompound>? Shape { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundWithDuplicateName
    {
        [Attr(PublicName = "duplicate")]
        public string? First { get; set; }

        [Attr(PublicName = "duplicate")]
        public string? Second { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateNameInCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithDuplicateName? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private class CompoundBaseWithName
    {
        [Attr(PublicName = "duplicate")]
        public string? FromBase { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class CompoundDerivedWithDuplicateName : CompoundBaseWithName
    {
        [Attr(PublicName = "duplicate")]
        public string? FromDerived { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateNameInDerivedCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundDerivedWithDuplicateName? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class NamedNestedCompound
    {
        [Attr]
        public string? FirstName { get; set; }

        [Attr(PublicName = "ZIP_Code")]
        public string? ZipCode { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithNamedNestedAttributes : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public NamedNestedCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class BaseWithInheritedCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public InnerCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DerivedWithInheritedCompound : BaseWithInheritedCompound;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ChainChild
    {
        [Attr]
        public int Scalar { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ChainCompound
    {
        [Attr(IsCompound = true)]
        public ChainChild? Child { get; set; }

        [Attr(IsCompound = true)]
        public List<ChainChild>? Children { get; set; }

        [Attr]
        public int? Number { get; set; }

        [Attr]
        public string? Text { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ChainResource : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ChainCompound? Compound { get; set; }

        [HasOne]
        public ChainResource? Related { get; set; }

        public string? NotExposed { get; set; }
    }
}
