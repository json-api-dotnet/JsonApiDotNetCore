using Castle.DynamicProxy;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
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
    public void Cannot_add_resource_with_abstract_compound_attribute()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add(typeof(ResourceWithAbstractCompoundType));

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("Resource inheritance is not supported on compound attributes.");
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

        // Act
        builder.Add(typeof(NonResource));

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        // TODO: Q: Can [NoResource] be used on compound types to suppress warning?
        // A: No, a type can't be both a resource and a complex attribute container (it has identity, or it hasn't).
        logLines[0].Should().Be(
            $"[WARNING] Skipping: Type '{typeof(NonResource)}' does not implement 'IIdentifiable'. Add [NoResource] to suppress this warning.");
    }

    [Fact]
    public void Logs_no_warning_when_adding_non_resource_type_with_suppression()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add(typeof(NonResourceWithSuppression));

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
    public void Logs_warning_when_adding_compound_attribute_that_has_no_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add<ResourceWithCompoundTypeThatHasNoAttributes, int>();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be($"[WARNING] Type '{typeof(CompoundTypeWithoutAttributes)}' does not contain any attributes.");
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

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithCompoundTypeThatHasNoAttributes : Identifiable<int>
    {
        [Attr(IsCompound = true)]
        public CompoundTypeWithoutAttributes CompoundType { get; set; } = new();
    }

    private sealed class CompoundTypeWithoutAttributes;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithAbstractCompoundType : Identifiable<int>
    {
        [Attr(IsCompound = true)]
        public AbstractCompoundType CompoundType { get; set; } = null!;
    }

    private abstract class AbstractCompoundType;

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

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DerivedAlternateToManyRelationshipName : AbstractBaseResource
    {
        [HasMany(PublicName = "derivedToMany")]
        public override ISet<AbstractBaseResource> BaseToMany { get; set; } = new HashSet<AbstractBaseResource>();
    }
}
