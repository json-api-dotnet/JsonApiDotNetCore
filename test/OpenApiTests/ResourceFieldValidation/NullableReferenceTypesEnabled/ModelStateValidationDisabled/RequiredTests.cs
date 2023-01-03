using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesEnabled.ModelStateValidationDisabled;

public sealed class RequiredTests
    : IClassFixture<OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>
        _testContext;

    public RequiredTests(
        OpenApiTestContext<ModelStateValidationDisabledStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtEnabledResourcesController>();
    }

    [Theory]
    [InlineData("nonNullableReferenceType")]
    [InlineData("nullableReferenceType")]
    [InlineData("valueType")]
    [InlineData("nullableValueType")]
    public async Task Schema_property_that_describes_attribute_in_create_resource_is_not_required(string jsonPropertyName)
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
    [InlineData("requiredNonNullableReferenceType")]
    [InlineData("requiredNullableReferenceType")]
    [InlineData("requiredValueType")]
    [InlineData("requiredNullableValueType")]
    public async Task Schema_property_that_describes_attribute_in_create_resource_is_required(string jsonPropertyName)
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
    [InlineData("requiredNonNullableToOne")]
    [InlineData("requiredNullableToOne")]
    [InlineData("requiredToMany")]
    public async Task Schema_property_that_describes_relationship_in_create_resource_is_required(string jsonPropertyName)
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
    public async Task Schema_property_that_describes_relationship_in_create_resource_is_not_required(string jsonPropertyName)
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
    public async Task No_schema_properties_for_relationships_when_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.resourceRelationshipsInPatchRequest.required");
    }
}
