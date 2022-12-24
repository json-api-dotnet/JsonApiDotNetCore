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

public sealed class RelationshipRequestTests
{
    private const string ChickenUrl = "http://localhost/chickens";

    [Fact]
    public async Task Can_exclude_optional_attributes()
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
                    NameOfCurrentFarm = "Cow and Chicken Farm",
                    Weight = 30,
                    HasProducedEggs = true
                }
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(ChickenUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""chickens"",
    ""attributes"": {
      ""nameOfCurrentFarm"": ""Cow and Chicken Farm"",
      ""weight"": 30,
      ""hasProducedEggs"": true
    }
  }
}");
    }

    [Theory]
    [InlineData(nameof(ChickenAttributesInResponse.NameOfCurrentFarm), "nameOfCurrentFarm")]
    [InlineData(nameof(ChickenAttributesInResponse.Weight), "weight")]
    [InlineData(nameof(ChickenAttributesInResponse.HasProducedEggs), "hasProducedEggs")]
    public async Task Cannot_exclude_required_attribute_when_performing_POST(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var attributesInPostRequest = new ChickenAttributesInPostRequest
        {
            Name = "Chicken",
            NameOfCurrentFarm = "Cow and Chicken Farm",
            Age = 10,
            Weight = 30,
            TimeAtCurrentFarmInDays = 100,
            HasProducedEggs = true
        };

        attributesInPostRequest.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new ChickenPostRequestDocument
        {
            Data = new ChickenDataInPostRequest
            {
                Attributes = attributesInPostRequest
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ChickenPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be($"Ignored property '{jsonName}' must have a value because it is required. Path 'data.attributes'.");
        }
    }

    [Fact]
    public async Task Can_exclude_attributes_that_are_required_for_POST_when_performing_PATCH()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new ChickenPatchRequestDocument
        {
            Data = new ChickenDataInPatchRequest
            {
                Id = "1",
                Attributes = new ChickenAttributesInPatchRequest
                {
                    Name = "Chicken",
                    Age = 10,
                    TimeAtCurrentFarmInDays = 100
                }
            }
        };

        // Act
        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPatchRequestDocument, ChickenAttributesInPatchRequest>(requestDocument))
        {
            await ApiResponse.TranslateAsync(async () => await apiClient.PatchChickenAsync(1, requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be(ChickenUrl + "/1");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""chickens"",
    ""id"": ""1"",
    ""attributes"": {
      ""name"": ""Chicken"",
      ""age"": 10,
      ""timeAtCurrentFarmInDays"": 100
    }
  }
}");
    }

    [Fact]
    public async Task Cannot_exclude_id_when_performing_PATCH()
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
                    Name = "Chicken",
                    NameOfCurrentFarm = "Cow and Chicken Farm",
                    Age = 10,
                    Weight = 30,
                    TimeAtCurrentFarmInDays = 100,
                    HasProducedEggs = true
                }
            }
        };

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PatchChickenAsync(1, requestDocument));

        // Assert
        await action.Should().ThrowAsync<JsonSerializationException>();
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Can_clear_nullable_attributes()
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
                    Name = null,
                    TimeAtCurrentFarmInDays = null,
                    NameOfCurrentFarm = "Cow and Chicken Farm",
                    Age = 10,
                    Weight = 30,
                    HasProducedEggs = true
                }
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument,
            chicken => chicken.Name, chicken => chicken.TimeAtCurrentFarmInDays))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(ChickenUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""chickens"",
    ""attributes"": {
      ""name"": null,
      ""nameOfCurrentFarm"": ""Cow and Chicken Farm"",
      ""age"": 10,
      ""weight"": 30,
      ""timeAtCurrentFarmInDays"": null,
      ""hasProducedEggs"": true
    }
  }
}");
    }

    [Fact]
    public async Task Cannot_clear_required_attribute_when_performing_POST()
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
                    Name = "Chicken",
                    NameOfCurrentFarm = null,
                    Age = 10,
                    Weight = 30,
                    TimeAtCurrentFarmInDays = 100,
                    HasProducedEggs = true
                }
            }
        };

        Func<Task<ChickenPrimaryResponseDocument?>> action;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<ChickenPostRequestDocument, ChickenAttributesInPostRequest>(requestDocument,
            chicken => chicken.NameOfCurrentFarm))
        {
            // Act
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));
        }

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be("Cannot write a null value for property 'nameOfCurrentFarm'. Property requires a value. Path 'data.attributes'.");
    }

    [Fact]
    public async Task Can_set_default_value_to_ValueType_attributes()
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
                    Name = "Chicken",
                    NameOfCurrentFarm = "Cow and Chicken Farm",
                    TimeAtCurrentFarmInDays = 100
                }
            }
        };

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostChickenAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(ChickenUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""chickens"",
    ""attributes"": {
      ""name"": ""Chicken"",
      ""nameOfCurrentFarm"": ""Cow and Chicken Farm"",
      ""age"": 0,
      ""weight"": 0,
      ""timeAtCurrentFarmInDays"": 100,
      ""hasProducedEggs"": false
    }
  }
}");
    }
}

