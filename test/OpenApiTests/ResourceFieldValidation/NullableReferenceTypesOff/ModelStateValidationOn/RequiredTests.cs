using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn;

public sealed class RequiredTests : IClassFixture<OpenApiTestContext<OpenApiStartup<NrtOffDbContext>, NrtOffDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NrtOffDbContext>, NrtOffDbContext> _testContext;

    public RequiredTests(OpenApiTestContext<OpenApiStartup<NrtOffDbContext>, NrtOffDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtOffResourcesController>();
    }

    [Theory]
    [InlineData("requiredReferenceType")]
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
    [InlineData("referenceType")]
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
    [InlineData("requiredToOne")]
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
    [InlineData("toOne")]
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
