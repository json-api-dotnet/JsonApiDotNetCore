using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff;

public sealed class CreateResourceTests : BaseOpenApiNSwagClientTests
{
    private readonly NrtOffMsvOffFakers _fakers = new();

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredReferenceType), "requiredReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.ValueType), "valueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_set_attribute_to_default_value(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        object? defaultValue = SetPropertyToDefaultValue(requestBody.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Attributes, attributePropertyName);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestBody));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.Should().Be(defaultValue));
        });
    }

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.ValueType), "valueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.NullableValueType), "nullableValueType")]
    public async Task Can_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestBody.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Attributes);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestBody));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredReferenceType), "requiredReferenceType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(AttributesInCreateResourceRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Cannot_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestBody.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Attributes);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestBody);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a default value for property '{jsonPropertyName}'. Property requires a non-default value. Path 'data.attributes'.");
    }

    [Theory]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.ToOne), "toOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.RequiredToOne), "requiredToOne")]
    public async Task Can_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        SetDataPropertyToNull(requestBody.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Relationships, relationshipPropertyName);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestBody));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.ToMany), "toMany")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        SetDataPropertyToNull(requestBody.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Relationships, relationshipPropertyName);

        // Act
        Func<Task> action = async () => await apiClient.PostResourceAsync(null, requestBody);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be(
            $"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.ToOne), "toOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.ToMany), "toMany")]
    public async Task Can_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestBody = new CreateResourceRequestDocument
        {
            Data = new DataInCreateResourceRequest
            {
                Attributes = _fakers.PostAttributes.GenerateOne(),
                Relationships = new RelationshipsInCreateResourceRequest
                {
                    ToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestBody.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Relationships);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(null, requestBody));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
