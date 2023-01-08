using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class UpdateResourceTests
{
    private readonly NrtOnMsvOffFakers _fakers = new();

    [Fact]
    public async Task Cannot_exclude_id()
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Id = "1",
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceDataInPatchRequest emptyDataObject = new();
        requestDocument.Data.Id = emptyDataObject.Id;

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        Func<Task> action = async () => await apiClient.PatchResourceAsync(999, requestDocument);

        // Assert
        await action.Should().ThrowAsync<JsonSerializationException>();
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPatchRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_exclude_attribute_that_is_required_in_create_resource(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Id = "1",
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceAttributesInPatchRequest emptyAttributesObject = new();
        object? defaultValue = emptyAttributesObject.GetPropertyValue(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePatchRequestDocument, ResourceAttributesInPatchRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(int.Parse(requestDocument.Data.Id), requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.RequiredNullableToOne), "requiredNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_exclude_relationship_that_is_required_in_create_resource(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Id = "1",
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPatchRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePatchRequestDocument, ResourceAttributesInPatchRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(int.Parse(requestDocument.Data.Id), requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
