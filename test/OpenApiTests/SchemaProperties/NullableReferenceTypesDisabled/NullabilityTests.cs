using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ChickensController>();
        testContext.UseController<HenHousesController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesDisabled";
    }

    [Theory]
    [InlineData("name")]
    [InlineData("timeAtCurrentFarmInDays")]
    public async Task Property_in_schema_for_attribute_of_resource_should_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(propertyName).With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });
        });
    }

    [Theory]
    [InlineData("nameOfCurrentFarm")]
    [InlineData("age")]
    [InlineData("weight")]
    [InlineData("hasProducedEggs")]
    public async Task Property_in_schema_for_attribute_of_should_not_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(propertyName).With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });
        });
    }

    [Theory]
    [InlineData("oldestChicken")]
    public async Task Property_in_schema_for_relationship_of_resource_should_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.henHouseRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data.oneOf[1].$ref").ShouldBeSchemaReferenceId("nullValue");
            });
        });
    }

    [Theory]
    [InlineData("allChickens")]
    [InlineData("firstChicken")]
    [InlineData("chickensReadyForLaying")]
    public async Task Data_property_in_schema_for_relationship_of_resource_should_not_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.henHouseRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data").ShouldNotContainPath("oneOf[1].$ref");
            });
        });
    }
}
