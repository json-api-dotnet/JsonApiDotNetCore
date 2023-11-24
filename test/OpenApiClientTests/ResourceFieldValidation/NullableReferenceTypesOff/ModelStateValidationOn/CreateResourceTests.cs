using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn;

public sealed class CreateResourceTests : BaseOpenApiClientTests
{
    private readonly NrtOffMsvOnFakers _fakers = new();

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), "referenceType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? defaultValue = SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector);

        // Act
        await ApiResponse.TranslateAsync(() => apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.Should().Be(defaultValue));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), "requiredReferenceType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = () => apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be($"Cannot write a null value for property '{jsonPropertyName}'. Property requires a value. Path 'data.attributes'.");
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(() => apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), "requiredReferenceType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = () => apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<InvalidOperationException> assertion = await action.Should().ThrowExactlyAsync<InvalidOperationException>();

        assertion.Which.Message.Should().Be(
            $"Required property '{attributePropertyName}' at JSON path 'data.attributes.{jsonPropertyName}' is not set. If sending its default value is intended, include it explicitly.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    public async Task Can_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(() => apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToOne), "requiredToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = () => apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(() => apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToOne), "requiredToOne")]
    public async Task Cannot_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = () => apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
    }
}
