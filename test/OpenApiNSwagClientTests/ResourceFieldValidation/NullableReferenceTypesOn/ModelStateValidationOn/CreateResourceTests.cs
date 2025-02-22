using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn;

public sealed class CreateResourceTests : BaseOpenApiNSwagClientTests
{
    private readonly NrtOnMsvOnFakers _fakers = new();

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.ValueType), "valueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_set_attribute_to_default_value(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        object? defaultValue = SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Attributes, attributePropertyName);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.Should().Be(defaultValue));
        });
    }

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    public async Task Cannot_set_attribute_to_default_value(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetPropertyToDefaultValue(requestDocument.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Attributes, attributePropertyName);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be($"Cannot write a null value for property '{jsonPropertyName}'. Property requires a value. Path 'data.attributes'.");
    }

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.ValueType), "valueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableValueType), "nullableValueType")]
    public async Task Can_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Attributes);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Cannot_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Attributes);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a default value for property '{jsonPropertyName}'. Property requires a non-default value. Path 'data.attributes'.");
    }

    [Theory]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NullableToOne), "nullableToOne")]
    public async Task Can_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Relationships, relationshipPropertyName);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNullableToOne), "requiredNullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.ToMany), "toMany")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetDataPropertyToNull(requestDocument.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Relationships, relationshipPropertyName);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.ToMany), "toMany")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Relationships);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(RelationshipsInCreateResourceRequest.RequiredNullableToOne), "requiredNullableToOne")]
    public async Task Cannot_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.ToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestDocument.Data.Relationships);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
    }
}
