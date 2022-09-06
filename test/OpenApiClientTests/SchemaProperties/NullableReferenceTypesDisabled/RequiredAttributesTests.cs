using System.Net;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class RequiredAttributesTests
{
    private const string HostPrefix = "http://localhost/";

    [Fact]
    public async Task Partial_posting_resource_with_explicitly_omitting_required_fields_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new ChickenPostRequestDocument
        {
            Data = new ChickenDataInPostRequest
            {
                Attributes = new ChickenAttributesInPostRequest
                {
                    HasProducedEggs = true
                }
            }
        };

        using (apiClient.RegisterAttributesForRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument,
            chicken => chicken.HasProducedEggs))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(HostPrefix + "chickens");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""chickens"",
    ""attributes"": {
      ""hasProducedEggs"": true
    }
  }
}");
    }

    [Fact]
    public async Task Partial_posting_resource_without_explicitly_omitting_required_fields_fails()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new ChickenPostRequestDocument
        {
            Data = new ChickenDataInPostRequest
            {
                Attributes = new ChickenAttributesInPostRequest
                {
                    Weight = 3
                }
            }
        };

        // Act
        Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'nameOfCurrentFarm'. Property requires a value. Path 'data.attributes'.");
    }

    [Fact]
    public async Task Patching_resource_with_missing_id_fails()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new ChickenPatchRequestDocument
        {
            Data = new ChickenDataInPatchRequest
            {
                Attributes = new ChickenAttributesInPatchRequest
                {
                    Age = 1
                }
            }
        };

        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PatchChickenAsync(1, requestDocument));

        // Assert
        await action.Should().ThrowAsync<JsonSerializationException>();
    }
}
