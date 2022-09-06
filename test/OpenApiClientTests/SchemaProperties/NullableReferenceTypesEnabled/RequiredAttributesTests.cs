using System.Net;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class RequiredAttributesTests
{
    private const string HostPrefix = "http://localhost/";

    [Fact]
    public async Task Partial_posting_resource_with_explicitly_omitting_required_fields_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        var requestDocument = new CowPostRequestDocument
        {
            Data = new CowDataInPostRequest
            {
                Attributes = new CowAttributesInPostRequest
                {
                    HasProducedMilk = true,
                    Weight = 1100
                }
            }
        };

        using (apiClient.RegisterAttributesForRequestDocument<CowPostRequestDocument, CowAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(HostPrefix + "cows");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""cows"",
    ""attributes"": {
      ""weight"": 1100,
      ""hasProducedMilk"": true
    }
  }
}");
    }

    [Fact]
    public async Task Partial_posting_resource_without_explicitly_omitting_required_fields_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        var requestDocument = new CowPostRequestDocument
        {
            Data = new CowDataInPostRequest
            {
                Attributes = new CowAttributesInPostRequest
                {
                    Weight = 1100,
                    Age = 5,
                    Name = "Cow 1",
                    NameOfCurrentFarm = "123",
                    NameOfPreviousFarm = "123"
                }
            }
        };

        // Act
        Func<Task<CowPrimaryResponseDocument?>>
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostCowAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'nickname'. Property requires a value. Path 'data.attributes'.");
    }
}
