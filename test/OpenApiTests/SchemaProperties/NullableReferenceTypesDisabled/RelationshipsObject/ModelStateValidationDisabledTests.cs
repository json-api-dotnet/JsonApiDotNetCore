using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

public sealed class ModelStateValidationDisabledTests
    : IClassFixture<OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>
        _testContext;

    public ModelStateValidationDisabledTests(
        OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<HenHousesController>();
    }

    [Theory]
    [InlineData("firstChicken")]
    [InlineData("chickensReadyForLaying")]
    public async Task Property_in_schema_for_relationships_in_POST_request_should_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.henHouseRelationshipsInPostRequest.required").With(propertySet =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredAttributes.Should().Contain(propertyName);
        });
    }

    [Theory]
    [InlineData("oldestChicken")]
    [InlineData("allChickens")]
    public async Task Property_in_schema_for_relationships_in_POST_request_should_not_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.henHouseRelationshipsInPostRequest.required").With(propertySet =>
        {
            var requiredProperties = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredProperties.Should().NotContain(propertyName);
        });
    }

    [Fact]
    public async Task Schema_for_relationships_in_PATCH_request_should_have_no_required_properties()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.henHouseRelationshipsInPatchRequest.required");
    }
}


