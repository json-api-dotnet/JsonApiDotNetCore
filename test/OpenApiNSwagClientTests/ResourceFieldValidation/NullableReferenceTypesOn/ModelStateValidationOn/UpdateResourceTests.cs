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

public sealed class UpdateResourceTests : BaseOpenApiNSwagClientTests
{
    private readonly NrtOnMsvOnFakers _fakers = new();

    [Fact]
    public async Task Cannot_omit_Id()
    {
        // Arrange
        var requestDocument = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Attributes = _fakers.PatchAttributes.GenerateOne(),
                Relationships = new RelationshipsInUpdateResourceRequest
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

        // Act
        Func<Task> action = async () => await apiClient.PatchResourceAsync(Unknown.StringId.Int32, null, requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();

        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Theory]
    [InlineData(nameof(AttributesInUpdateResourceRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.ValueType), "valueType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(AttributesInUpdateResourceRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_omit_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = Unknown.StringId.Int32,
                Attributes = _fakers.PatchAttributes.GenerateOne(),
                Relationships = new RelationshipsInUpdateResourceRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Attributes, attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using IDisposable _ = apiClient.WithPartialAttributeSerialization<UpdateResourceRequestDocument, AttributesInUpdateResourceRequest>(requestDocument);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(requestDocument.Data.Id, null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.RequiredNullableToOne), "requiredNullableToOne")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.ToMany), "toMany")]
    [InlineData(nameof(RelationshipsInUpdateResourceRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_omit_relationship(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = Unknown.StringId.Int32,
                Attributes = _fakers.PatchAttributes.GenerateOne(),
                Relationships = new RelationshipsInUpdateResourceRequest
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

        SetPropertyToInitialValue(requestDocument.Data.Relationships, relationshipPropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(requestDocument.Data.Id, null, requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
