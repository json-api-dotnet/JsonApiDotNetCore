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

public sealed class RelationshipsRequestTests
{
    private const string CowStableUrl = "http://localhost/cowStables";

    [Fact]
    public async Task Can_exclude_optional_relationships()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        var requestDocument = new CowStablePostRequestDocument
        {
            Data = new CowStableDataInPostRequest
            {
                Relationships = new CowStableRelationshipsInPostRequest
                {
                    OldestCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    FirstCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    FavoriteCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    AllCows = new ToManyCowInRequest
                    {
                        Data = new List<CowIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = CowResourceType.Cows
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(CowStableUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""cowStables"",
    ""relationships"": {
      ""oldestCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""firstCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""favoriteCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""allCows"": {
        ""data"": [
          {
            ""type"": ""cows"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }

    [Theory]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.OldestCow), "oldestCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.FirstCow), "firstCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.FavoriteCow), "favoriteCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.AllCows), "allCows")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStableRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            OldestCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            FirstCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            FavoriteCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            CowsReadyForMilking = new ToManyCowInRequest
            {
                Data = new List<CowIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = CowResourceType.Cows
                    }
                }
            },
            AllCows = new ToManyCowInRequest
            {
                Data = new List<CowIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = CowResourceType.Cows
                    }
                }
            }
        };

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new CowStablePostRequestDocument
        {
            Data = new CowStableDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<CowStablePostRequestDocument, CowStableRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be($"Ignored property '{jsonName}' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Theory]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.OldestCow), "oldestCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.FirstCow), "firstCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.FavoriteCow), "favoriteCow")]
    [InlineData(nameof(CowStableRelationshipsInPostRequest.AllCows), "allCows")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        CowStableRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            OldestCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            FirstCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            AlbinoCow = new NullableToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            FavoriteCow = new ToOneCowInRequest
            {
                Data = new CowIdentifier
                {
                    Id = "1",
                    Type = CowResourceType.Cows
                }
            },
            CowsReadyForMilking = new ToManyCowInRequest
            {
                Data = new List<CowIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = CowResourceType.Cows
                    }
                }
            },
            AllCows = new ToManyCowInRequest
            {
                Data = new List<CowIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = CowResourceType.Cows
                    }
                }
            }
        };

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new CowStablePostRequestDocument
        {
            Data = new CowStableDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        // Act
        Func<Task<CowStablePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Cannot write a null value for property '{jsonName}'. Property requires a value. Path 'data.relationships'.");
    }

    [Fact]
    public async Task Can_exclude_relationships_that_are_required_for_POST_when_performing_PATCH()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        var requestDocument = new CowStablePatchRequestDocument
        {
            Data = new CowStableDataInPatchRequest
            {
                Id = "1",
                Type = CowStableResourceType.CowStables,
                Relationships = new CowStableRelationshipsInPatchRequest
                {
                    AlbinoCow = new NullableToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    CowsReadyForMilking = new ToManyCowInRequest
                    {
                        Data = new List<CowIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = CowResourceType.Cows
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PatchCowStableAsync(1, requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be(CowStableUrl + "/1");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""cowStables"",
    ""id"": ""1"",
    ""relationships"": {
      ""albinoCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""cowsReadyForMilking"": {
        ""data"": [
          {
            ""type"": ""cows"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }

    [Fact]
    public async Task Can_clear_nullable_relationship()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClient(wrapper.HttpClient);

        var requestDocument = new CowStablePostRequestDocument
        {
            Data = new CowStableDataInPostRequest
            {
                Relationships = new CowStableRelationshipsInPostRequest
                {
                    AlbinoCow = new NullableToOneCowInRequest
                    {
                        Data = null
                    },
                    OldestCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    FirstCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    FavoriteCow = new ToOneCowInRequest
                    {
                        Data = new CowIdentifier
                        {
                            Id = "1",
                            Type = CowResourceType.Cows
                        }
                    },
                    CowsReadyForMilking = new ToManyCowInRequest
                    {
                        Data = new List<CowIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = CowResourceType.Cows
                            }
                        }
                    },
                    AllCows = new ToManyCowInRequest
                    {
                        Data = new List<CowIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = CowResourceType.Cows
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PostCowStableAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(CowStableUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""cowStables"",
    ""relationships"": {
      ""oldestCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""firstCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""albinoCow"": {
        ""data"": null
      },
      ""favoriteCow"": {
        ""data"": {
          ""type"": ""cows"",
          ""id"": ""1""
        }
      },
      ""cowsReadyForMilking"": {
        ""data"": [
          {
            ""type"": ""cows"",
            ""id"": ""1""
          }
        ]
      },
      ""allCows"": {
        ""data"": [
          {
            ""type"": ""cows"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }
}

