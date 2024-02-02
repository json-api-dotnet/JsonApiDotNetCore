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
        document.Should().ContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesSchema =>
        {
            attributesSchema.Should().ContainPath($"properties.{jsonPropertyName}");
            attributesSchema.Should().ContainPath("required").With(propertySet => propertySet.Should().ContainArrayElement(jsonPropertyName));
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
        document.Should().ContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesSchema =>
        {
            attributesSchema.Should().ContainPath($"properties.{jsonPropertyName}");
            attributesSchema.Should().ContainPath("required").With(propertySet => propertySet.Should().NotContainArrayElement(jsonPropertyName));
        });
    }

    [Theory]
    [InlineData("requiredToOne")]
    public async Task Schema_property_for_relationship_is_required_for_creating_resource(string jsonPropertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsSchema =>
        {
            relationshipsSchema.Should().ContainPath($"properties.{jsonPropertyName}");
            relationshipsSchema.Should().ContainPath("required").With(propertySet => propertySet.Should().ContainArrayElement(jsonPropertyName));
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
        document.Should().ContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsSchema =>
        {
            relationshipsSchema.Should().ContainPath($"properties.{jsonPropertyName}");
            relationshipsSchema.Should().ContainPath("required").With(propertySet => propertySet.Should().NotContainArrayElement(jsonPropertyName));
        });
    }

    [Fact]
    public async Task No_attribute_schema_properties_are_required_for_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().NotContainPath("components.schemas.resourceAttributesInPatchRequest.required");
    }

    [Fact]
    public async Task No_relationship_schema_properties_are_required_for_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().NotContainPath("components.schemas.resourceRelationshipsInPatchRequest.required");
    }
}
