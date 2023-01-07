using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class RequiredTests : IClassFixture<OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext>>
{
    private readonly OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext> _testContext;

    public RequiredTests(OpenApiTestContext<MsvOffStartup<NrtOnDbContext>, NrtOnDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtOnResourcesController>();
    }

    [Theory]
    [InlineData("requiredNonNullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("requiredValueType")]
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
    [InlineData("nonNullableReferenceType")]
    [InlineData("nullableReferenceType")]
    [InlineData("valueType")]
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
    [InlineData("requiredNonNullableToOne")]
    [InlineData("requiredNullableToOne")]
    [InlineData("requiredToMany")]
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
    [InlineData("nonNullableToOne")]
    [InlineData("nullableToOne")]
    [InlineData("toMany")]
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
