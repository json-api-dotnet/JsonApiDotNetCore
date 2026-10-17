using Castle.DynamicProxy;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph;

public sealed class ResourceGraphBuilderTests
{
    [Fact]
    public void Resource_without_public_name_gets_pluralized_with_naming_convention_applied()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithAttribute, long>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithAttribute>();

        resourceType.PublicName.Should().Be("resourceWithAttributes");
    }

    [Fact]
    public void Attribute_without_public_name_gets_naming_convention_applied()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithAttribute, long>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithAttribute>();

        AttrAttribute attribute = resourceType.GetAttributeByPropertyName(nameof(ResourceWithAttribute.PrimaryValue));
        attribute.PublicName.Should().Be("primaryValue");
    }

    [Fact]
    public void HasOne_relationship_without_public_name_gets_naming_convention_applied()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithAttribute, long>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithAttribute>();

        RelationshipAttribute relationship = resourceType.GetRelationshipByPropertyName(nameof(ResourceWithAttribute.PrimaryChild));
        relationship.PublicName.Should().Be("primaryChild");
    }

    [Fact]
    public void HasMany_relationship_without_public_name_gets_naming_convention_applied()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithAttribute, long>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithAttribute>();

        RelationshipAttribute relationship = resourceType.GetRelationshipByPropertyName(nameof(ResourceWithAttribute.TopLevelChildren));
        relationship.PublicName.Should().Be("topLevelChildren");
    }

    [Fact]
    public void Cannot_use_duplicate_resource_name()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithHasOneRelationship, long>("duplicate");

        // Act
        Action action = () => builder.Add<ResourceWithAttribute, long>("duplicate");

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Resource '{typeof(ResourceWithHasOneRelationship)}' and '{typeof(ResourceWithAttribute)}' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_use_duplicate_attribute_name()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add<ResourceWithDuplicateAttrPublicName, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Properties '{typeof(ResourceWithDuplicateAttrPublicName)}.Value1' and " +
            $"'{typeof(ResourceWithDuplicateAttrPublicName)}.Value2' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_use_duplicate_relationship_name()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add<ResourceWithDuplicateRelationshipPublicName, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Properties '{typeof(ResourceWithDuplicateRelationshipPublicName)}.PrimaryChild' and " +
            $"'{typeof(ResourceWithDuplicateRelationshipPublicName)}.Children' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_use_duplicate_attribute_and_relationship_name()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add<ResourceWithDuplicateAttrRelationshipPublicName, long>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Properties '{typeof(ResourceWithDuplicateAttrRelationshipPublicName)}.Value' and " +
            $"'{typeof(ResourceWithDuplicateAttrRelationshipPublicName)}.Children' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_add_resource_that_implements_only_non_generic_IIdentifiable()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add(typeof(ResourceWithoutId));

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>()
            .WithMessage($"Resource type '{typeof(ResourceWithoutId)}' implements 'IIdentifiable', but not 'IIdentifiable<TId>'.");
    }

    [Fact]
    public void Can_remove_existing_resource_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithHasOneRelationship, long>();
        builder.Add<ResourceWithAttribute, long>();

        // Act
        builder.Remove<ResourceWithHasOneRelationship>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        resourceGraph.GetResourceTypes().Should().ContainSingle().Which.ClrType.Should().Be<ResourceWithAttribute>();
    }

    [Fact]
    public void Can_remove_missing_resource_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithAttribute, long>();

        // Act
        builder.Remove<ResourceWithHasManyRelationship>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        resourceGraph.GetResourceTypes().Should().ContainSingle().Which.ClrType.Should().Be<ResourceWithAttribute>();
    }

    [Fact]
    public void Can_remove_non_resource_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceWithAttribute, long>();

        // Act
        builder.Remove(typeof(NonResource));

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        resourceGraph.GetResourceTypes().Should().ContainSingle().Which.ClrType.Should().Be<ResourceWithAttribute>();
    }

    [Fact]
    public void Cannot_build_graph_with_missing_related_HasOne_resource()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<ResourceWithHasOneRelationship, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Resource type '{typeof(ResourceWithHasOneRelationship)}' " +
            $"depends on '{typeof(ResourceWithAttribute)}', which was not added to the resource graph.");
    }

    [Fact]
    public void Cannot_build_graph_with_missing_related_HasMany_resource()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<ResourceWithHasManyRelationship, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Resource type '{typeof(ResourceWithHasManyRelationship)}' " +
            $"depends on '{typeof(ResourceWithAttribute)}', which was not added to the resource graph.");
    }

    [Fact]
    public void Cannot_build_graph_with_different_attribute_name_in_derived_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<AbstractBaseResource, long>();
        builder.Add<DerivedAlternateAttributeName, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("Attribute 'baseValue' from base type " +
            $"'{typeof(AbstractBaseResource)}' does not exist in derived type '{typeof(DerivedAlternateAttributeName)}'.");
    }

    [Fact]
    public void Cannot_build_graph_with_different_ToOne_relationship_name_in_derived_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<AbstractBaseResource, long>();
        builder.Add<DerivedAlternateToOneRelationshipName, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("Relationship 'baseToOne' from base type " +
            $"'{typeof(AbstractBaseResource)}' does not exist in derived type '{typeof(DerivedAlternateToOneRelationshipName)}'.");
    }

    [Fact]
    public void Cannot_build_graph_with_different_ToMany_relationship_name_in_derived_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<AbstractBaseResource, long>();
        builder.Add<DerivedAlternateToManyRelationshipName, long>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("Relationship 'baseToMany' from base type " +
            $"'{typeof(AbstractBaseResource)}' does not exist in derived type '{typeof(DerivedAlternateToManyRelationshipName)}'.");
    }

    [Fact]
    public void Logs_warning_when_adding_non_resource_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        builder.Add<ResourceWithAttribute, long>();
        builder.Add(typeof(NonResource));

        // Act
        builder.Build();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be($"[WARNING] Skipping: Type '{typeof(NonResource)}' does not implement 'IIdentifiable'. " +
            $"Add [NoResource] or call {nameof(ResourceGraphBuilder.SuppressWarningNotIdentifiable)} to suppress this warning.");
    }

    [Fact]
    public void Logs_no_warning_when_adding_non_resource_type_with_suppression()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        builder.Add<ResourceWithAttribute, long>();
        builder.Add(typeof(NonResourceWithSuppression));

        // Act
        builder.Build();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().BeEmpty();
    }

    [Fact]
    public void Logs_no_warning_when_adding_non_resource_type_with_suppression_by_method()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        builder.Add<ResourceWithAttribute, long>();
        builder.Add(typeof(NonResource));
        builder.SuppressWarningNotIdentifiable<NonResource>();

        // Act
        builder.Build();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().BeEmpty();
    }

    [Fact]
    public void Logs_warning_when_adding_resource_without_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add<ResourceWithHasOneRelationship, long>();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be($"[WARNING] Type '{typeof(ResourceWithHasOneRelationship)}' does not contain any attributes.");
    }

    [Fact]
    public void Logs_warning_on_empty_graph()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Build();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be("[WARNING] The resource graph is empty.");
    }

    [Fact]
    public void Resolves_correct_type_for_lazy_loading_proxy()
    {
        // Arrange
        var options = new JsonApiOptions();

        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        builder.Add<ResourceOfInt32, int>();
        IResourceGraph resourceGraph = builder.Build();

        var proxyGenerator = new ProxyGenerator();
        var proxy = proxyGenerator.CreateClassProxy<ResourceOfInt32>();

        // Act
        ResourceType resourceType = resourceGraph.GetResourceType(proxy.GetType());

        // Assert
        resourceType.ClrType.Should().Be<ResourceOfInt32>();
    }

    [Fact]
    public void Can_override_capabilities_on_Id_property()
    {
        // Arrange
        var options = new JsonApiOptions
        {
            DefaultAttrCapabilities = AttrCapabilities.None
        };

        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithIdOverride, long>();

        // Assert
        IResourceGraph resourceGraph = builder.Build();
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithIdOverride>();

        AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<>.Id));
        idAttribute.Capabilities.Should().Be(AttrCapabilities.AllowFilter);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithHasOneRelationship : Identifiable<long>
    {
        [HasOne]
        public ResourceWithAttribute? PrimaryChild { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithHasManyRelationship : Identifiable<long>
    {
        [HasMany]
        public ISet<ResourceWithAttribute> Children { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithAttribute : Identifiable<long>
    {
        [Attr]
        public string? PrimaryValue { get; set; }

        [HasOne]
        public ResourceWithAttribute? PrimaryChild { get; set; }

        [HasMany]
        public ISet<ResourceWithAttribute> TopLevelChildren { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateAttrPublicName : Identifiable<long>
    {
        [Attr(PublicName = "duplicate")]
        public string? Value1 { get; set; }

        [Attr(PublicName = "duplicate")]
        public string? Value2 { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateRelationshipPublicName : Identifiable<long>
    {
        [HasOne(PublicName = "duplicate")]
        public ResourceWithHasOneRelationship? PrimaryChild { get; set; }

        [HasMany(PublicName = "duplicate")]
        public ISet<ResourceWithAttribute> Children { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateAttrRelationshipPublicName : Identifiable<long>
    {
        [Attr(PublicName = "duplicate")]
        public string? Value { get; set; }

        [HasMany(PublicName = "duplicate")]
        public ISet<ResourceWithAttribute> Children { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    private sealed class ResourceWithoutId : IIdentifiable
    {
        public string? StringId { get; set; }
        public string? LocalId { get; set; }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class NonResource;

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [NoResource]
    private sealed class NonResourceWithSuppression;

    // ReSharper disable once ClassCanBeSealed.Global
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class ResourceOfInt32 : Identifiable<int>
    {
        [Attr]
        public string? StringValue { get; set; }

        [HasOne]
        public ResourceOfInt32? PrimaryChild { get; set; }

        [HasMany]
        public IList<ResourceOfInt32> Children { get; set; } = new List<ResourceOfInt32>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithIdOverride : Identifiable<long>
    {
        [Attr(Capabilities = AttrCapabilities.AllowFilter)]
        public override long Id { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class AbstractBaseResource : Identifiable<long>
    {
        [Attr(PublicName = "baseValue")]
        public virtual int Value { get; set; }

        [HasOne(PublicName = "baseToOne")]
        public virtual AbstractBaseResource? BaseToOne { get; set; }

        [HasMany(PublicName = "baseToMany")]
        public virtual ISet<AbstractBaseResource> BaseToMany { get; set; } = new HashSet<AbstractBaseResource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DerivedAlternateAttributeName : AbstractBaseResource
    {
        [Attr(PublicName = "derivedValue")]
        public override int Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DerivedAlternateToOneRelationshipName : AbstractBaseResource
    {
        [HasOne(PublicName = "derivedToOne")]
        public override AbstractBaseResource? BaseToOne { get; set; }
    }

    [Fact]
    public void Discovers_compound_types_recursively()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithCompoundAttributes, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithCompoundAttributes>();
        AttrAttribute contentAttr = resourceType.GetAttributeByPropertyName(nameof(ResourceWithCompoundAttributes.Content));
        contentAttr.IsCompound.Should().BeTrue();
        contentAttr.CompoundType.Should().NotBeNull();
        contentAttr.ContainingType.Should().Be(resourceType);

        CompoundType rootCompoundType = contentAttr.CompoundType!;
        rootCompoundType.ClrType.Should().Be(typeof(TestRootCompound));
        rootCompoundType.Attributes.Should().HaveCount(2);

        AttrAttribute nameAttr = rootCompoundType.GetAttributeByPropertyName(nameof(TestRootCompound.Name));
        nameAttr.IsCompound.Should().BeTrue();
        nameAttr.CompoundType.Should().NotBeNull();
        nameAttr.ContainingType.Should().Be(rootCompoundType);

        CompoundType nameCompoundType = nameAttr.CompoundType!;
        nameCompoundType.ClrType.Should().Be(typeof(TestChildCompound));
        AttrAttribute firstAttr = nameCompoundType.GetAttributeByPropertyName(nameof(TestChildCompound.FirstName));
        firstAttr.IsCompound.Should().BeFalse();
        firstAttr.CompoundType.Should().BeNull();
        firstAttr.ContainingType.Should().Be(nameCompoundType);

        AttrAttribute phonesAttr = rootCompoundType.GetAttributeByPropertyName(nameof(TestRootCompound.PhoneNumbers));
        phonesAttr.IsCompound.Should().BeTrue();
        phonesAttr.IsCollection.Should().BeTrue();
        phonesAttr.CompoundType.Should().NotBeNull();
        phonesAttr.CompoundType!.ClrType.Should().Be(typeof(TestPhoneCompound));

        resourceGraph.GetCompoundTypes().Should().HaveCount(3);
        resourceGraph.GetCompoundType(typeof(TestRootCompound)).Should().Be(rootCompoundType);
        resourceGraph.GetCompoundType(typeof(TestChildCompound)).Should().Be(nameCompoundType);
        resourceGraph.GetCompoundType(typeof(TestPhoneCompound)).Should().Be(phonesAttr.CompoundType);
    }

    [Fact]
    public void Automatically_strips_AllowSort_from_compound_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithSortableCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        ResourceType resourceType = resourceGraph.GetResourceType<ResourceWithSortableCompound>();
        AttrAttribute attr = resourceType.GetAttributeByPropertyName(nameof(ResourceWithSortableCompound.Compound));
        attr.Capabilities.HasFlag(AttrCapabilities.AllowSort).Should().BeFalse();
    }

    [Fact]
    public void Cannot_use_identifiable_type_as_compound_attribute()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithIdentifiableAsCompound, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{nameof(ResourceWithIdentifiableAsCompound)}.{nameof(ResourceWithIdentifiableAsCompound.Child)}' implements IIdentifiable and must be exposed as a JSON:API relationship using [HasOne] or [HasMany], not as a compound attribute.");
    }

    [Fact]
    public void Cannot_use_resource_type_as_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithRegisteredResourceAsCompound, long>();
        builder.Add<SimpleResource, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{nameof(ResourceWithRegisteredResourceAsCompound)}.{nameof(ResourceWithRegisteredResourceAsCompound.Child)}' implements IIdentifiable and must be exposed as a JSON:API relationship using [HasOne] or [HasMany], not as a compound attribute.");
    }

    [Fact]
    public void Cannot_annotate_compound_type_with_ResourceAttribute()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithAnnotatedCompound, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Type '{typeof(CompoundWithResourceAttribute)}' is a compound type and cannot be annotated with [Resource].");
    }

    [Fact]
    public void Cannot_use_HasOne_inside_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithHasOneInCompound, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{nameof(CompoundWithHasOne)}.{nameof(CompoundWithHasOne.Related)}' in compound type '{nameof(CompoundWithHasOne)}' cannot be annotated with [HasOneAttribute]. Compound types cannot contain relationships.");
    }

    [Fact]
    public void Cannot_use_HasMany_inside_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithHasManyInCompound, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{nameof(CompoundWithHasMany)}.{nameof(CompoundWithHasMany.Related)}' in compound type '{nameof(CompoundWithHasMany)}' cannot be annotated with [HasManyAttribute]. Compound types cannot contain relationships.");
    }

    [Fact]
    public void Cannot_use_EagerLoad_inside_compound_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithEagerLoadInCompound, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Property '{nameof(CompoundWithEagerLoad)}.{nameof(CompoundWithEagerLoad.SubObject)}' in compound type '{nameof(CompoundWithEagerLoad)}' cannot be annotated with [EagerLoad].");
    }

    [Fact]
    public void Detects_direct_circular_reference_in_compound_types()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithDirectCycle, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Circular reference detected: compound type '{typeof(DirectCycleCompound)}' references itself directly or indirectly.");
    }

    [Fact]
    public void Detects_indirect_circular_reference_in_compound_types()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithIndirectCycle, long>();
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Circular reference detected: compound type '{typeof(IndirectCycleCompoundA)}' references itself directly or indirectly.");
    }

    [Fact]
    public void Supports_empty_base_class_in_compound_type_hierarchy()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        builder.Add<ResourceWithDerivedCompound, long>();
        IResourceGraph resourceGraph = builder.Build();

        // Assert
        resourceGraph.FindCompoundType(typeof(DerivedCompound)).Should().NotBeNull();
        CompoundType compoundType = resourceGraph.GetCompoundType(typeof(DerivedCompound));
        compoundType.Attributes.Should().ContainSingle(attr => attr.Property.Name == nameof(DerivedCompound.Value));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithSortableCompound : Identifiable<long>
    {
        [Attr(Capabilities = AttrCapabilities.All, IsCompound = true)]
        public TestChildCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SimpleResource : Identifiable<long>;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithIdentifiableAsCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public SimpleResource? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithRegisteredResourceAsCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public SimpleResource? Child { get; set; }
    }

    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]
    public sealed class CompoundWithResourceAttribute
    {
        [Attr]
        public string? Text { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithAnnotatedCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithResourceAttribute? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundWithHasOne
    {
        [HasOne]
        public SimpleResource? Related { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithHasOneInCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithHasOne? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundWithHasMany
    {
        [HasMany]
        public ISet<SimpleResource> Related { get; set; } = new HashSet<SimpleResource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithHasManyInCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithHasMany? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompoundWithEagerLoad
    {
        [EagerLoad]
        public TestChildCompound? SubObject { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithEagerLoadInCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public CompoundWithEagerLoad? Child { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DirectCycleCompound
    {
        [Attr(IsCompound = true)]
        public DirectCycleCompound? Next { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithDirectCycle : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public DirectCycleCompound? Cycle { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IndirectCycleCompoundA
    {
        [Attr(IsCompound = true)]
        public IndirectCycleCompoundB? Next { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IndirectCycleCompoundB
    {
        [Attr(IsCompound = true)]
        public IndirectCycleCompoundA? Next { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithIndirectCycle : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public IndirectCycleCompoundA? Cycle { get; set; }
    }

    public abstract class EmptyCompoundBase;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DerivedCompound : EmptyCompoundBase
    {
        [Attr]
        public string? Value { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithDerivedCompound : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public DerivedCompound? Compound { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithCompoundAttributes : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public TestRootCompound? Content { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestRootCompound
    {
        [Attr(IsCompound = true)]
        public TestChildCompound? Name { get; set; }

        [Attr(IsCompound = true)]
        public List<TestPhoneCompound> PhoneNumbers { get; set; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestChildCompound
    {
        [Attr]
        public string? FirstName { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestPhoneCompound
    {
        [Attr]
        public string? Number { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DerivedAlternateToManyRelationshipName : AbstractBaseResource
    {
        [HasMany(PublicName = "derivedToMany")]
        public override ISet<AbstractBaseResource> BaseToMany { get; set; } = new HashSet<AbstractBaseResource>();
    }
}
