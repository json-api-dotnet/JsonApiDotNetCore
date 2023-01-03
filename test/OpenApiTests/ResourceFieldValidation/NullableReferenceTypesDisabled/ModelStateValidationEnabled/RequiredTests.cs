using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesDisabled.ModelStateValidationEnabled;

public sealed class RequiredTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public RequiredTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NrtDisabledResourcesController>();
    }

    [Fact]
    public async Task Schema_property_for_reference_type_attribute_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.referenceType");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("referenceType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_required_reference_type_attribute_is_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.requiredReferenceType");

            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithElement("requiredReferenceType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_value_type_attribute_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.valueType");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("valueType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_required_value_type_attribute_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.requiredValueType");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("requiredValueType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_nullable_value_type_attribute_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.nullableValueType");
            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("nullableValueType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_required_nullable_value_type_attribute_is_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceAttributesInPostRequest").With(attributesObjectSchema =>
        {
            attributesObjectSchema.ShouldContainPath("properties.requiredNullableValueType");

            attributesObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithElement("requiredNullableValueType"));
        });
    }

    [Fact]
    public async Task Schema_property_for_to_one_relationship_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath("properties.toOne");
            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("toOne"));
        });
    }

    [Fact]
    public async Task Schema_property_for_required_to_one_relationship_is_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath("properties.requiredToOne");

            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithElement("requiredToOne"));
        });
    }

    [Fact]
    public async Task Schema_property_for_to_many_relationship_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath("properties.toMany");
            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("toMany"));
        });
    }

    [Fact]
    public async Task Schema_property_for_required_to_many_relationship_is_not_required()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.resourceRelationshipsInPostRequest").With(relationshipsObjectSchema =>
        {
            relationshipsObjectSchema.ShouldContainPath("properties.requiredToMany");
            relationshipsObjectSchema.ShouldContainPath("required").With(propertySet => propertySet.ShouldBeArrayWithoutElement("requiredToMany"));
        });
    }

    [Fact]
    public async Task No_schema_properties_for_attributes_are_required_when_updating_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldNotContainPath("components.schemas.resourceAttributesInPatchRequest.required");
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
