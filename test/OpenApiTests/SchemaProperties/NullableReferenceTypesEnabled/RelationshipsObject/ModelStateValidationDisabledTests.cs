using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

public sealed class ModelStateValidationDisabledTests
    : IClassFixture<OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>
        _testContext;

    public ModelStateValidationDisabledTests(
        OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CowStablesController>();
    }

    [Theory]
    [InlineData("firstCow")]
    [InlineData("allCows")]
    [InlineData("favoriteCow")]
    public async Task Property_in_schema_for_relationships_in_POST_request_should_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowStableRelationshipsInPostRequest.required").With(propertySet =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(propertySet.GetRawText());

            requiredAttributes.Should().Contain(propertyName);
        });
    }

    [Theory]
    [InlineData("oldestCow")]
    [InlineData("cowsReadyForMilking")]
    [InlineData("albinoCow")]
    public async Task Property_in_schema_for_relationships_in_POST_request_should_not_be_required(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowStableRelationshipsInPostRequest.required").With(propertySet =>
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
        document.ShouldNotContainPath("components.schemas.cowStableRelationshipsInPatchRequest.required");
    }
}


