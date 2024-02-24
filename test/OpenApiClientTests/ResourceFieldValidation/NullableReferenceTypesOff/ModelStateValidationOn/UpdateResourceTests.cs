using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn;

public sealed class UpdateResourceTests : BaseOpenApiClientTests
{
    private readonly NrtOffMsvOnFakers _fakers = new();

    [Fact]
    public async Task Cannot_omit_Id()
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOnClient(wrapper.HttpClient);

        // Act
        Func<Task> action = async () => await apiClient.PatchResourceAsync(Unknown.StringId.Int32, null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPatchRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredReferenceType), "requiredReferenceType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(ResourceAttributesInPatchRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Id = Unknown.StringId.Int32,
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
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

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<ResourcePatchRequestDocument, ResourceAttributesInPatchRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(() => apiClient.PatchResourceAsync(requestDocument.Data.Id, null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.RequiredToOne), "requiredToOne")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPatchRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePatchRequestDocument
        {
            Data = new ResourceDataInPatchRequest
            {
                Id = Unknown.StringId.Int32,
                Attributes = _fakers.PatchAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPatchRequest
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
        await ApiResponse.TranslateAsync(() => apiClient.PatchResourceAsync(requestDocument.Data.Id, null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
