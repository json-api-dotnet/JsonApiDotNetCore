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
        builder.Add<ResourceWithAttribute, int>();

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
        builder.Add<ResourceWithAttribute, int>();

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
        builder.Add<ResourceWithAttribute, int>();

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
        builder.Add<ResourceWithAttribute, int>();

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
        builder.Add<ResourceWithHasOneRelationship, int>("duplicate");

        // Act
        Action action = () => builder.Add<ResourceWithAttribute, int>("duplicate");

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
        Action action = () => builder.Add<ResourceWithDuplicateAttrPublicName, int>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            $"Properties '{typeof(ResourceWithDuplicateAttrPublicName)}.Value1' and " +
            $"'{typeof(ResourceWithDuplicateAttrPublicName)}.Value2' both use public name 'duplicate'.");
    }

    [Fact]
    public void Cannot_use_duplicate_relationship_name()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        // Act
        Action action = () => builder.Add<ResourceWithDuplicateRelationshipPublicName, int>();

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
        Action action = () => builder.Add<ResourceWithDuplicateAttrRelationshipPublicName, int>();

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
    public void Cannot_build_graph_with_missing_related_HasOne_resource()
    {
        // Arrange
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        builder.Add<ResourceWithHasOneRelationship, int>();

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

        builder.Add<ResourceWithHasManyRelationship, int>();

        // Act
        Action action = () => builder.Build();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Resource type '{typeof(ResourceWithHasManyRelationship)}' " +
            $"depends on '{typeof(ResourceWithAttribute)}', which was not added to the resource graph.");
    }

    [Fact]
    public void Logs_warning_when_adding_non_resource_type()
    {
        // Arrange
        var options = new JsonApiOptions();
        var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add(typeof(NonResource));

        // Assert
        loggerFactory.Logger.Messages.ShouldHaveCount(1);

        FakeLoggerFactory.FakeLogMessage message = loggerFactory.Logger.Messages.ElementAt(0);
        message.LogLevel.Should().Be(LogLevel.Warning);
        message.Text.Should().Be($"Skipping: Type '{typeof(NonResource)}' does not implement 'IIdentifiable'.");
    }

    [Fact]
    public void Logs_warning_when_adding_resource_without_attributes()
    {
        // Arrange
        var options = new JsonApiOptions();
        var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Add<ResourceWithHasOneRelationship, int>();

        // Assert
        loggerFactory.Logger.Messages.ShouldHaveCount(1);

        FakeLoggerFactory.FakeLogMessage message = loggerFactory.Logger.Messages.ElementAt(0);
        message.LogLevel.Should().Be(LogLevel.Warning);
        message.Text.Should().Be($"Type '{typeof(ResourceWithHasOneRelationship)}' does not contain any attributes.");
    }

    [Fact]
    public void Logs_warning_on_empty_graph()
    {
        // Arrange
        var options = new JsonApiOptions();
        var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);
        var builder = new ResourceGraphBuilder(options, loggerFactory);

        // Act
        builder.Build();

        // Assert
        loggerFactory.Logger.Messages.ShouldHaveCount(1);

        FakeLoggerFactory.FakeLogMessage message = loggerFactory.Logger.Messages.ElementAt(0);
        message.LogLevel.Should().Be(LogLevel.Warning);
        message.Text.Should().Be("The resource graph is empty.");
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
        resourceType.ClrType.Should().Be(typeof(ResourceOfInt32));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithHasOneRelationship : Identifiable<int>
    {
        [HasOne]
        public ResourceWithAttribute? PrimaryChild { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithHasManyRelationship : Identifiable<int>
    {
        [HasMany]
        public ISet<ResourceWithAttribute> Children { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithAttribute : Identifiable<int>
    {
        [Attr]
        public string? PrimaryValue { get; set; }

        [HasOne]
        public ResourceWithAttribute? PrimaryChild { get; set; }

        [HasMany]
        public ISet<ResourceWithAttribute> TopLevelChildren { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateAttrPublicName : Identifiable<int>
    {
        [Attr(PublicName = "duplicate")]
        public string? Value1 { get; set; }

        [Attr(PublicName = "duplicate")]
        public string? Value2 { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateRelationshipPublicName : Identifiable<int>
    {
        [HasOne(PublicName = "duplicate")]
        public ResourceWithHasOneRelationship? PrimaryChild { get; set; }

        [HasMany(PublicName = "duplicate")]
        public ISet<ResourceWithAttribute> Children { get; set; } = new HashSet<ResourceWithAttribute>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithDuplicateAttrRelationshipPublicName : Identifiable<int>
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
    private sealed class NonResource
    {
    }

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
}
