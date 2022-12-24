using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<HenHousesController>();
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
    public async Task Property_in_schema_for_relationship_of_resource_should_not_be_nullable(string propertyName)
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


