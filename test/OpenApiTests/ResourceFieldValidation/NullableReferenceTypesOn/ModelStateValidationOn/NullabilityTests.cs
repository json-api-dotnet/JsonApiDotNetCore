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
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData("nullableReferenceType")]
    [InlineData("nullableValueType")]
    public async Task Schema_property_for_attribute_is_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.resourceAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.Should().ContainPath(jsonPropertyName).With(schemaProperty =>
            {
                schemaProperty.Should().ContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
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
        document.Should().ContainPath("components.schemas.resourceAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.Should().ContainPath(jsonPropertyName).With(schemaProperty =>
            {
                schemaProperty.Should().NotContainPath("nullable");
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
        document.Should().ContainPath("components.schemas.relationshipsInCreateResourceRequest.properties").With(schemaProperties =>
        {
            schemaProperties.Should().ContainPath($"{jsonPropertyName}.allOf[0].$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.Should().ContainPath($"components.schemas.{schemaReferenceId}.properties.data").With(relationshipDataSchema =>
                {
                    relationshipDataSchema.Should().ContainPath("nullable").With(nullableProperty => nullableProperty.Should().Be(true));
                });
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
        document.Should().ContainPath("components.schemas.relationshipsInCreateResourceRequest.properties").With(schemaProperties =>
        {
            schemaProperties.Should().ContainPath($"{jsonPropertyName}.allOf[0].$ref").WithSchemaReferenceId(schemaReferenceId =>
            {
                document.Should().ContainPath($"components.schemas.{schemaReferenceId}.properties.data").With(relationshipDataSchema =>
                {
                    relationshipDataSchema.Should().NotContainPath("nullable");
                });
            });
        });
    }
}
