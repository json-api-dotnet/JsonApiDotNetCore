using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class CreateResourceTests : BaseOpenApiClientTests
{
    private readonly NrtOnMsvOffFakers _fakers = new();

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_set_attribute_to_default_value(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        object? defaultValue = SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.Should().Be(defaultValue));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    public async Task Cannot_set_attribute_to_default_value(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().StartWith($"Cannot write a null value for property '{jsonPropertyName}'.");
        assertion.Which.Message.Should().EndWith("Path 'data.attributes'.");
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), "nullableValueType")]
    public async Task Can_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Cannot_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<InvalidOperationException> assertion = await action.Should().ThrowExactlyAsync<InvalidOperationException>();

        assertion.Which.Message.Should().Be(
            $"Required property '{attributePropertyName}' at JSON path 'data.attributes.{jsonPropertyName}' is not set. If sending its default value is intended, include it explicitly.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), "requiredNullableToOne")]
    public async Task Can_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), "requiredNullableToOne")]
    public async Task Can_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    public async Task Can_omit_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    public async Task Can_omit_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    public async Task Cannot_omit_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    public async Task Cannot_omit_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
    }
}
