using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff;

public sealed class UpdateResourceTests : BaseOpenApiNSwagClientTests
{
    private readonly NrtOnMsvOffFakers _fakers = new();

    [Fact]
    public async Task Cannot_omit_Id()
    {
        // Arrange
        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Attributes = _fakers.PatchAttributes.GenerateOne(),
                Relationships = new RelationshipsInUpdateResourceRequest
                {
                    NonNullableToOne = _fakers.ToOne.GenerateOne(),
                    RequiredNonNullableToOne = _fakers.ToOne.GenerateOne(),
                    NullableToOne = _fakers.NullableToOne.GenerateOne(),
                    RequiredNullableToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        // Act
        Func<Task> action = async () => await apiClient.PatchResourceAsync(Unknown.StringId.Int32, null, requestBody);

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
        var requestBody = new UpdateResourceRequestDocument
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
                    RequiredNullableToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestBody.Data.Attributes, attributePropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Attributes);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(requestBody.Data.Id, null, requestBody));

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
        var requestBody = new UpdateResourceRequestDocument
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
                    RequiredNullableToOne = _fakers.NullableToOne.GenerateOne(),
                    ToMany = _fakers.ToMany.GenerateOne(),
                    RequiredToMany = _fakers.ToMany.GenerateOne()
                }
            }
        };

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOffClient(wrapper.HttpClient);

        SetPropertyToInitialValue(requestBody.Data.Relationships, relationshipPropertyName);
        apiClient.MarkAsTracked(requestBody.Data.Relationships);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(requestBody.Data.Id, null, requestBody));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
