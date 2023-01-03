using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesEnabled.ModelStateValidationDisabled;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>
        _testContext;

    public NullabilityTests(
        OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtEnabledResourcesController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/ResourceFieldValidation/NullableReferenceTypesEnabled/ModelStateValidationDisabled";
    }

    [Theory]
    [InlineData("nonNullableReferenceType")]
    [InlineData("requiredNonNullableReferenceType")]
    [InlineData("valueType")]
    [InlineData("requiredValueType")]
    public async Task Schema_property_that_describes_attribute_is_not_nullable(string jsonPropertyName)
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
    [InlineData("nullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("nullableValueType")]
    [InlineData("requiredNullableValueType")]
    public async Task Schema_property_that_describes_attribute_is_nullable(string jsonPropertyName)
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
    [InlineData("nonNullableToOne")]
    [InlineData("requiredNonNullableToOne")]
    [InlineData("toMany")]
    [InlineData("requiredToMany")]
    public async Task Schema_property_that_describes_relationship_is_not_nullable(string jsonPropertyName)
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

    [Theory]
    [InlineData("nullableToOne")]
    [InlineData("requiredNullableToOne")]
    public async Task Schema_property_that_describes_relationship_is_nullable(string jsonPropertyName)
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
}
