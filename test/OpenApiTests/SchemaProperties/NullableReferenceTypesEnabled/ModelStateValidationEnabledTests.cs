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

    [Theory]
    [InlineData("name")]
    [InlineData("nameOfCurrentFarm")]
    [InlineData("nickname")]
    [InlineData("weight")]
    [InlineData("hasProducedMilk")]
    public async Task Property_in_schema_for_attributes_in_POST_request_should_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInPostRequest.required").With(propertySet =>
        {
            var requiredProperties = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredProperties.Should().Contain(propertyName);
        });
    }

    [Theory]
    [InlineData("age")]
    [InlineData("timeAtCurrentFarmInDays")]
    public async Task Property_in_schema_for_attributes_in_POST_request_should_not_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInPostRequest.required").With(propertySet =>
        {
            var requiredProperties = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredProperties.Should().NotContain(propertyName);
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
