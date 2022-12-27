using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled;

/// <summary>
/// Should consider if the shape of the two tests here is more favourable over the test with the same name in the RequestTests suite. The drawback of the
/// form here is that the expected json string is less easy to read. However the win is that this form allows us to run the tests in a [Theory]. This is
/// relevant because each of these properties represent unique test cases. In the other test form, it is not clear which properties are tested without.
/// For instance: here in Can_exclude_optional_relationships it is immediately clear that the properties we omit are those in the inline data.
/// </summary>
public sealed class AlternativeFormRequestTests
{
    private const string HenHouseUrl = "http://localhost/henHouses";

    private readonly Dictionary<string, string> _partials = new()
    {
        {
            nameof(HenHouseRelationshipsInPostRequest.OldestChicken), @"""oldestChicken"": {
        ""data"": {
          ""type"": ""chickens"",
          ""id"": ""1""
        }
      }"
        },
        {
            nameof(HenHouseRelationshipsInPostRequest.FirstChicken), @"""firstChicken"": {
        ""data"": {
          ""type"": ""chickens"",
          ""id"": ""1""
        }
      }"
        },
        {
            nameof(HenHouseRelationshipsInPostRequest.AllChickens), @"""allChickens"": {
        ""data"": [
          {
            ""type"": ""chickens"",
            ""id"": ""1""
          }
        ]
      }"
        },

        {
            nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying), @"""chickensReadyForLaying"": {
        ""data"": [
          {
            ""type"": ""chickens"",
            ""id"": ""1""
          }
        ]
      }"
        }
    };

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.OldestChicken))]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.AllChickens))]
    public async Task Can_exclude_optional_relationships(string propertyName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHouseRelationshipsInPostRequest relationshipsObject = new()
        {
            OldestChicken = new NullableToOneChickenInRequest
            {
                Data = new ChickenIdentifier
                {
                    Id = "1",
                    Type = ChickenResourceType.Chickens
                }
            },
            FirstChicken = new ToOneChickenInRequest
            {
                Data = new ChickenIdentifier
                {
                    Id = "1",
                    Type = ChickenResourceType.Chickens
                }
            },
            AllChickens = new ToManyChickenInRequest
            {
                Data = new List<ChickenIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = ChickenResourceType.Chickens
                    }
                }
            },
            ChickensReadyForLaying = new ToManyChickenInRequest
            {
                Data = new List<ChickenIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = ChickenResourceType.Chickens
                    }
                }
            }
        };

        relationshipsObject.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = relationshipsObject
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(HenHouseUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        string body = GetRelationshipsObjectWithSinglePropertyOmitted(propertyName);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""henHouses"",
    ""relationships"": " + body + @"
  }
}");
    }

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.FirstChicken))]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying))]
    public async Task Can_exclude_relationships_that_are_required_for_POST_when_performing_PATCH(string propertyName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var relationshipsObject = new HenHouseRelationshipsInPatchRequest
        {
            OldestChicken = new NullableToOneChickenInRequest
            {
                Data = new ChickenIdentifier
                {
                    Id = "1",
                    Type = ChickenResourceType.Chickens
                }
            },
            FirstChicken = new ToOneChickenInRequest
            {
                Data = new ChickenIdentifier
                {
                    Id = "1",
                    Type = ChickenResourceType.Chickens
                }
            },
            AllChickens = new ToManyChickenInRequest
            {
                Data = new List<ChickenIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = ChickenResourceType.Chickens
                    }
                }
            },
            ChickensReadyForLaying = new ToManyChickenInRequest
            {
                Data = new List<ChickenIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = ChickenResourceType.Chickens
                    }
                }
            }
        };

        relationshipsObject.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new HenHousePatchRequestDocument
        {
            Data = new HenHouseDataInPatchRequest
            {
                Id = "1",
                Type = HenHouseResourceType.HenHouses,
                Relationships = relationshipsObject
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PatchHenHouseAsync(1, requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be(HenHouseUrl + "/1");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        string serializedRelationshipsObject = GetRelationshipsObjectWithSinglePropertyOmitted(propertyName);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""henHouses"",
    ""id"": ""1"",
    ""relationships"": " + serializedRelationshipsObject + @"
  }
}");
    }

    private string GetRelationshipsObjectWithSinglePropertyOmitted(string excludeProperty)
    {
        string partial = "";

        foreach ((string key, string relationshipJsonPartial) in _partials)
        {
            if (excludeProperty == key)
            {
                continue;
            }

            if (partial.Length > 0)
            {
                partial += ",\n      ";
            }

            partial += relationshipJsonPartial;
        }

        return @"{
      " + partial + @"
    }";
    }
}
