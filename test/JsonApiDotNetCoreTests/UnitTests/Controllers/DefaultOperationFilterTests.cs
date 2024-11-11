using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Controllers;

public sealed class DefaultOperationFilterTests
{
    // @formatter:wrap_chained_method_calls chop_always
    // @formatter:wrap_before_first_method_call true

    private static readonly IResourceGraph ResourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
        .Add<AbstractBaseType, long>()
        .Add<ConcreteBaseType, long>()
        .Add<ConcreteDerivedType, long>()
        .Build();

    // @formatter:wrap_before_first_method_call restore
    // @formatter:wrap_chained_method_calls restore

    [Theory]
    [InlineData(WriteOperationKind.CreateResource)]
    [InlineData(WriteOperationKind.UpdateResource)]
    [InlineData(WriteOperationKind.DeleteResource)]
    [InlineData(WriteOperationKind.SetRelationship)]
    [InlineData(WriteOperationKind.AddToRelationship)]
    [InlineData(WriteOperationKind.RemoveFromRelationship)]
    public void Operations_enabled_on_abstract_base_type_are_implicitly_enabled_on_derived_types(WriteOperationKind writeOperation)
    {
        // Arrange
        ResourceType abstractBaseType = ResourceGraph.GetResourceType<AbstractBaseType>();
        ResourceType concreteBaseType = ResourceGraph.GetResourceType<ConcreteBaseType>();
        ResourceType concreteDerivedType = ResourceGraph.GetResourceType<ConcreteDerivedType>();

        var filter = new FakeOperationFilter(resourceType => resourceType.Equals(abstractBaseType));

        // Act
        bool abstractBaseIsEnabled = filter.IsEnabled(abstractBaseType, writeOperation);
        bool concreteBaseIsEnabled = filter.IsEnabled(concreteBaseType, writeOperation);
        bool concreteDerivedIsEnabled = filter.IsEnabled(concreteDerivedType, writeOperation);

        // Assert
        abstractBaseIsEnabled.Should().BeTrue();
        concreteBaseIsEnabled.Should().BeTrue();
        concreteDerivedIsEnabled.Should().BeTrue();
    }

    [Theory]
    [InlineData(WriteOperationKind.CreateResource)]
    [InlineData(WriteOperationKind.UpdateResource)]
    [InlineData(WriteOperationKind.DeleteResource)]
    [InlineData(WriteOperationKind.SetRelationship)]
    [InlineData(WriteOperationKind.AddToRelationship)]
    [InlineData(WriteOperationKind.RemoveFromRelationship)]
    public void Operations_enabled_on_concrete_base_type_are_implicitly_enabled_on_derived_types(WriteOperationKind writeOperation)
    {
        // Arrange
        ResourceType abstractBaseType = ResourceGraph.GetResourceType<AbstractBaseType>();
        ResourceType concreteBaseType = ResourceGraph.GetResourceType<ConcreteBaseType>();
        ResourceType concreteDerivedType = ResourceGraph.GetResourceType<ConcreteDerivedType>();

        var filter = new FakeOperationFilter(resourceType => resourceType.Equals(concreteBaseType));

        // Act
        bool abstractBaseIsEnabled = filter.IsEnabled(abstractBaseType, writeOperation);
        bool concreteBaseIsEnabled = filter.IsEnabled(concreteBaseType, writeOperation);
        bool concreteDerivedIsEnabled = filter.IsEnabled(concreteDerivedType, writeOperation);

        // Assert
        abstractBaseIsEnabled.Should().BeFalse();
        concreteBaseIsEnabled.Should().BeTrue();
        concreteDerivedIsEnabled.Should().BeTrue();
    }

    private sealed class FakeOperationFilter : DefaultOperationFilter
    {
        private readonly Func<ResourceType, bool> _isResourceTypeEnabled;

        public FakeOperationFilter(Func<ResourceType, bool> isResourceTypeEnabled)
        {
            ArgumentGuard.NotNull(isResourceTypeEnabled);

            _isResourceTypeEnabled = isResourceTypeEnabled;
        }

        protected override JsonApiEndpoints? GetJsonApiEndpoints(ResourceType resourceType)
        {
            return _isResourceTypeEnabled(resourceType) ? JsonApiEndpoints.All : JsonApiEndpoints.None;
        }
    }

    private abstract class AbstractBaseType : Identifiable<long>;

    private class ConcreteBaseType : AbstractBaseType;

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class ConcreteDerivedType : ConcreteBaseType;
}
