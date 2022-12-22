using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtEnabledModelsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesEnabled/RelationshipsObject";
    }

    [Theory]
    [InlineData("nullableHasOne")]
    public async Task Property_in_schema_for_relationship_of_resource_should_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.nrtEnabledModelRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data.oneOf[1].$ref").ShouldBeSchemaReferenceId("nullValue");
            });
        });
    }

    [Theory]
    [InlineData("hasOne")]
    [InlineData("requiredHasOne")]
    [InlineData("hasMany")]
    [InlineData("requiredHasMany")]
    [InlineData("nullableRequiredHasOne")]
    public async Task Property_in_schema_for_relationship_of_resource_should_not_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.nrtEnabledModelRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{propertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data").ShouldNotContainPath("oneOf[1].$ref");
            });
        });
    }
}

