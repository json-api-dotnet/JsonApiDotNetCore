using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class ModelStateValidationEnabledTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> _testContext;

    public ModelStateValidationEnabledTests(
        OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CowsController>();
    }

    [Fact]
    public async Task Produces_expected_required_property_in_schema_for_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInPostRequest.required").With(requiredElement =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(requiredElement.GetRawText());
            requiredAttributes.ShouldHaveCount(5);

            requiredAttributes.Should().Contain("name");
            requiredAttributes.Should().Contain("nameOfCurrentFarm");
            requiredAttributes.Should().Contain("nickname");
            requiredAttributes.Should().Contain("weight");
            requiredAttributes.Should().Contain("hasProducedMilk");
        });
    }
}

