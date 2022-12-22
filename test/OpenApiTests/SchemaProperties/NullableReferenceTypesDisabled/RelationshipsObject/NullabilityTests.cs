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

        testContext.UseController<NrtDisabledModelsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesDisabled/RelationshipsObject";
    }

    [Theory]
    [InlineData("hasOne")]
    public async Task Property_in_schema_for_relationship_of_resource_should_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.nrtDisabledModelRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data.oneOf[1].$ref").ShouldBeSchemaReferenceId("nullValue");
            });
        });
    }

    [Theory]
    [InlineData("hasMany")]
    [InlineData("requiredHasOne")]
    [InlineData("requiredHasMany")]
    public async Task Property_in_schema_for_relationship_of_resource_should_not_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.nrtDisabledModelRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data").ShouldNotContainPath("oneOf[1].$ref");
            });
        });
    }
}

