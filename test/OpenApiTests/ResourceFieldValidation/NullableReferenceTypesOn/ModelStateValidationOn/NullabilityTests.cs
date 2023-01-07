using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn;

public sealed class NullabilityTests : IClassFixture<OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtOnResourcesController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/ResourceFieldValidation/NullableReferenceTypesOn/ModelStateValidationOn";
    }

    [Theory]
    [InlineData("nullableReferenceType")]
    [InlineData("nullableValueType")]
    public async Task Schema_property_for_attribute_is_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(jsonPropertyName).With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });
        });
    }

    [Theory]
    [InlineData("nonNullableReferenceType")]
    [InlineData("requiredNonNullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("valueType")]
    [InlineData("requiredValueType")]
    [InlineData("requiredNullableValueType")]
    public async Task Schema_property_for_attribute_is_not_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(jsonPropertyName).With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });
        });
    }

    [Theory]
    [InlineData("nullableToOne")]
    public async Task Schema_property_for_relationship_is_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{jsonPropertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data.oneOf[1].$ref").ShouldBeSchemaReferenceId("nullValue");
            });
        });
    }

    [Theory]
    [InlineData("nonNullableToOne")]
    [InlineData("requiredNonNullableToOne")]
    [InlineData("requiredNullableToOne")]
    [InlineData("toMany")]
    [InlineData("requiredToMany")]
    public async Task Schema_property_for_relationship_is_not_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath($"{jsonPropertyName}.$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.ShouldContainPath($"components.schemas.{schemaReferenceId}.properties.data").ShouldNotContainPath("oneOf[1].$ref");
            });
        });
    }
}
