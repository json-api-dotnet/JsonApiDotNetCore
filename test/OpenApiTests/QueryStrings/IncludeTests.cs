using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.QueryStrings;

public sealed class IncludeTests : IClassFixture<OpenApiTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

    public IncludeTests(OpenApiTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OnlyGetSingleNodeController>();
    }

    [Fact]
    public async Task Discriminator_is_generated_for_all_resource_types_when_subset_of_endpoints_is_exposed()
    {
        // Arrange
        var resourceGraph = _testContext.Factory.Services.GetRequiredService<IResourceGraph>();
        int count = resourceGraph.GetResourceTypes().Count;

        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            var discriminatorRefIds = new List<string>();

            schemasElement.Should().ContainPath("dataInResponse.discriminator").With(discriminatorElement =>
            {
                discriminatorElement.Should().HaveProperty("propertyName", "type");

                discriminatorElement.Should().ContainPath("mapping").With(mappingElement =>
                {
                    foreach (JsonProperty jsonProperty in mappingElement.EnumerateObject())
                    {
                        string discriminatorRefId = jsonProperty.Value.GetSchemaReferenceId();
                        discriminatorRefIds.Add(discriminatorRefId);
                    }
                });
            });

            discriminatorRefIds.Should().HaveCount(count);

            foreach (string discriminatorRefId in discriminatorRefIds)
            {
                schemasElement.Should().ContainPath($"{discriminatorRefId}.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInResponse");
            }
        });
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class OnlyGetSingleNodeController(
        IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Node, long> resourceService)
        : BaseJsonApiController<Node, long>(options, resourceGraph, loggerFactory, resourceService)
    {
        [HttpGet("{id}")]
        public override Task<IActionResult> GetAsync(long id, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, cancellationToken);
        }
    }
}
