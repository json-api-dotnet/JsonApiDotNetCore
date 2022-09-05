using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class ModelStateValidationDisabledTests
    : IClassFixture<OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>
        _testContext;

    public ModelStateValidationDisabledTests(
        OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;
        testContext.UseController<ChickensController>();
    }

    [Fact]
    public async Task Resource_when_ModelStateValidation_is_disabled_produces_expected_required_property_in_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInPostRequest.required").With(requiredElement =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(requiredElement.GetRawText());
            requiredAttributes.ShouldNotBeNull();

            requiredAttributes.Should().Contain("nameOfCurrentFarm");
            requiredAttributes.Should().Contain("weight");
            requiredAttributes.Should().Contain("hasProducedEggs");

            requiredAttributes.ShouldHaveCount(3);
        });
    }
}
