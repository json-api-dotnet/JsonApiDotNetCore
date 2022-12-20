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

    [Theory]
    [InlineData("nameOfCurrentFarm")]
    [InlineData("weight")]
    [InlineData("hasProducedEggs")]
    public async Task Property_in_schema_for_attributes_in_POST_request_should_be_required(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInPostRequest.required").With(propertySet =>
        {
            var requiredProperties = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredProperties.Should().Contain(attributeName);
        });
    }

    [Theory]
    [InlineData("name")]
    [InlineData("age")]
    [InlineData("timeAtCurrentFarmInDays")]
    public async Task Property_in_schema_for_attributes_in_POST_request_should_not_be_required(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInPostRequest.required").With(propertySet =>
        {
            var requiredProperties = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredProperties.Should().NotContain(attributeName);
        });
    }

    [Fact]
    public async Task Schema_for_attributes_in_PATCH_request_should_have_no_required_properties()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.chickenAttributesInPatchRequest.required");
    }
}
