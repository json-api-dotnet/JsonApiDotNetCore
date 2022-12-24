using System.Net;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class RequestTests
{
    private const string HenHouseUrl = "http://localhost/henHouses";

    [Fact]
    public async Task Can_exclude_optional_relationships()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = new HenHouseRelationshipsInPostRequest
                {
                    FirstChicken = new ToOneChickenInRequest
                    {
                        Data = new ChickenIdentifier
                        {
                            Id = "1",
                            Type = ChickenResourceType.Chickens
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
                }
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

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""henHouses"",
    ""relationships"": {
      ""firstChicken"": {
        ""data"": {
          ""type"": ""chickens"",
          ""id"": ""1""
        }
      },
      ""chickensReadyForLaying"": {
        ""data"": [
          {
            ""type"": ""chickens"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.FirstChicken), "firstChicken")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying), "chickensReadyForLaying")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHouseRelationshipsInPostRequest relationshipsInPostDocument = new()
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

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<HenHousePostRequestDocument, HenHouseRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be($"Ignored property '{jsonName}' must have a value because it is required. Path 'data.relationships'.");
        }
    }

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.FirstChicken), "firstChicken")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying), "chickensReadyForLaying")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHouseRelationshipsInPostRequest relationshipsInPostDocument = new()
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

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        // Act
        Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

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
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new HenHousePatchRequestDocument
        {
            Data = new HenHouseDataInPatchRequest
            {
                Id = "1",
                Type = HenHouseResourceType.HenHouses,
                Relationships = new HenHouseRelationshipsInPatchRequest
                {
                    OldestChicken = new NullableToOneChickenInRequest
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
                    }
                }
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

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""henHouses"",
    ""id"": ""1"",
    ""relationships"": {
      ""oldestChicken"": {
        ""data"": {
          ""type"": ""chickens"",
          ""id"": ""1""
        }
      },
      ""allChickens"": {
        ""data"": [
          {
            ""type"": ""chickens"",
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
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = new HenHouseRelationshipsInPostRequest
                {
                    OldestChicken = new NullableToOneChickenInRequest
                    {
                        Data = null
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
                }
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

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""henHouses"",
    ""relationships"": {
      ""oldestChicken"": {
        ""data"": null
      },
      ""firstChicken"": {
        ""data"": {
          ""type"": ""chickens"",
          ""id"": ""1""
        }
      },
      ""allChickens"": {
        ""data"": [
          {
            ""type"": ""chickens"",
            ""id"": ""1""
          }
        ]
      },
      ""chickensReadyForLaying"": {
        ""data"": [
          {
            ""type"": ""chickens"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.FirstChicken), "firstChicken")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.AllChickens), "allChickens")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying), "chickensReadyForLaying")]
    public async Task Cannot_clear_non_nullable_relationships_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHouseRelationshipsInPostRequest relationshipsInPostDocument = new()
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

        PropertyInfo relationshipToClearPropertyInfo = relationshipsInPostDocument.GetType().GetProperties().Single(property => property.Name == propertyName);
        object relationshipToClear = relationshipToClearPropertyInfo.GetValue(relationshipsInPostDocument)!;
        relationshipToClear.SetPropertyToDefaultValue("Data");

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        Func<Task<HenHousePrimaryResponseDocument?>> action;

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<HenHousePostRequestDocument, HenHouseRelationshipsInPostRequest>(requestDocument,
            model => model.FirstChicken, model => model.AllChickens, model => model.ChickensReadyForLaying))
        {
            // Act
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));
        }

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonName}'.");
    }

    [Theory]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.FirstChicken), "firstChicken")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.AllChickens), "allChickens")]
    [InlineData(nameof(HenHouseRelationshipsInPostRequest.ChickensReadyForLaying), "chickensReadyForLaying")]
    public async Task Cannot_clear_non_nullable_relationships_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClient(wrapper.HttpClient);

        HenHouseRelationshipsInPostRequest relationshipsInPostDocument = new()
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

        PropertyInfo relationshipToClearPropertyInfo = relationshipsInPostDocument.GetType().GetProperties().Single(property => property.Name == propertyName);
        object relationshipToClear = relationshipToClearPropertyInfo.GetValue(relationshipsInPostDocument)!;
        relationshipToClear.SetPropertyToDefaultValue("Data");

        var requestDocument = new HenHousePostRequestDocument
        {
            Data = new HenHouseDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        // Act
        Func<Task<HenHousePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostHenHouseAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonName}'.");
    }
}

