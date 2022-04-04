using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

public sealed class IncompleteResourceGraphTests
{
    [Fact]
    public void Fails_when_derived_type_is_missing_in_resource_graph()
    {
        // Arrange
        var options = new JsonApiOptions();

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Fruit, long>().Build();

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = resourceGraph.GetResourceType<Fruit>()
        };

        var linkBuilder = new FakeLinkBuilder();
        var metaBuilder = new FakeMetaBuilder();
        var resourceDefinitionAccessor = new FakeResourceDefinitionAccessor();
        var sparseFieldSetCache = new SparseFieldSetCache(Array.Empty<IQueryConstraintProvider>(), resourceDefinitionAccessor);
        var requestQueryStringAccessor = new FakeRequestQueryStringAccessor();
        var evaluatedIncludeCache = new EvaluatedIncludeCache();

        var responseModelAdapter = new ResponseModelAdapter(request, options, linkBuilder, metaBuilder, resourceDefinitionAccessor, evaluatedIncludeCache,
            sparseFieldSetCache, requestQueryStringAccessor);

        var banana = new Banana();

        // Act
        Action action = () => responseModelAdapter.Convert(banana);

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Type '{typeof(Banana)}' does not exist in the resource graph.");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private abstract class Fruit : Identifiable<long>
    {
        [Attr]
        public bool IsRipe { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Banana : Fruit
    {
        [Attr]
        public int Length { get; set; }
    }
}
