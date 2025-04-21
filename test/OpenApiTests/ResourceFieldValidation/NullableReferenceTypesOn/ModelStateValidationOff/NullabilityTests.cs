using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class NullabilityTests : IClassFixture<OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext>>
{
    private readonly OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<NrtOnResourcesController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData("nullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("nullableValueType")]
    [InlineData("requiredNullableValueType")]
    public async Task Schema_property_for_attribute_is_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInResourceResponse.allOf[1].properties").With(schemaProperties =>
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
    [InlineData("valueType")]
    [InlineData("requiredValueType")]
    public async Task Schema_property_for_attribute_is_not_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.attributesInResourceResponse.allOf[1].properties").With(schemaProperties =>
        {
            schemaProperties.Should().ContainPath(jsonPropertyName).With(schemaProperty =>
            {
                schemaProperty.Should().NotContainPath("nullable");
            });
        });
    }

    [Theory]
    [InlineData("nullableToOne")]
    [InlineData("requiredNullableToOne")]
    public async Task Schema_property_for_relationship_is_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateResourceRequest.allOf[1].properties").With(schemaProperties =>
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
    [InlineData("toMany")]
    [InlineData("requiredToMany")]
    public async Task Schema_property_for_relationship_is_not_nullable(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.relationshipsInCreateResourceRequest.allOf[1].properties").With(schemaProperties =>
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
