using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn;

public sealed class RequiredTests : IClassFixture<OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext> _testContext;

    public RequiredTests(OpenApiTestContext<OpenApiStartup<NrtOnDbContext>, NrtOnDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtOnResourcesController>();
    }

    [Theory]
    [InlineData("nonNullableReferenceType")]
    [InlineData("requiredNonNullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("requiredNullableValueType")]
    public async Task Schema_property_for_attribute_is_required_for_creating_resource(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath($"properties.{jsonPropertyName}");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithElement(jsonPropertyName));
        });
    }

    [Theory]
    [InlineData("nullableReferenceType")]
    [InlineData("valueType")]
    [InlineData("requiredValueType")]
    [InlineData("nullableValueType")]
    public async Task Schema_property_for_attribute_is_not_required_for_creating_resource(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath($"properties.{jsonPropertyName}");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement(jsonPropertyName));
        });
    }

    [Theory]
    [InlineData("nonNullableToOne")]
    [InlineData("requiredNonNullableToOne")]
    [InlineData("requiredNullableToOne")]
    public async Task Schema_property_for_relationship_is_required_for_creating_resource(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath($"properties.{jsonPropertyName}");
            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithElement(jsonPropertyName));
        });
    }

    [Theory]
    [InlineData("nullableToOne")]
    [InlineData("toMany")]
    [InlineData("requiredToMany")]
    public async Task Schema_property_for_relationship_is_not_required_for_creating_resource(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath($"properties.{jsonPropertyName}");
            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement(jsonPropertyName));
        });
    }

    [Fact]
    public async Task No_attribute_schema_properties_are_required_when_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.resourceAttributesInPatchRequest.required");
    }

    [Fact]
    public async Task No_relationship_schema_properties_are_required_when_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.resourceRelationshipsInPatchRequest.required");
    }
}
