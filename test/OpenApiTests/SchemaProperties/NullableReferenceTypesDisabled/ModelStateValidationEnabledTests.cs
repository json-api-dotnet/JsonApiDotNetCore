using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class ModelStateValidationEnabledTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public ModelStateValidationEnabledTests(
        OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ChickensController>();
    }

    [Fact]
    public async Task Produces_expected_required_property_in_schema_for_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInPostRequest.required").With(requiredElement =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(requiredElement.GetRawText());
            requiredAttributes.ShouldHaveCount(3);

            requiredAttributes.Should().Contain("nameOfCurrentFarm");
            requiredAttributes.Should().Contain("weight");
            requiredAttributes.Should().Contain("hasProducedEggs");
        });
    }
}

