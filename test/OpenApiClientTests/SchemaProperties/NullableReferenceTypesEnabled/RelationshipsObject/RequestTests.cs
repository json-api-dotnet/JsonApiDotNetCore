using System.Net;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject;

public sealed class RequestTests
{
    private const string NrtEnabledModelUrl = "http://localhost/nrtEnabledModels";

    [Fact]
    public async Task Can_exclude_optional_relationships()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtEnabledModelPostRequestDocument()
        {
            Data = new NrtEnabledModelDataInPostRequest
            {
                Relationships = new NrtEnabledModelRelationshipsInPostRequest
                {
                    HasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    RequiredHasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    NullableRequiredHasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    RequiredHasMany = new ToManyRelationshipModelInRequest
                    {
                        Data = new List<RelationshipModelIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = RelationshipModelResourceType.RelationshipModels
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtEnabledModelAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(NrtEnabledModelUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtEnabledModels"",
    ""relationships"": {
      ""hasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""requiredHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""nullableRequiredHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""requiredHasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }

    [Theory]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.HasOne), "hasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.NullableRequiredHasOne), "nullableRequiredHasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClientRelationshipsObject(wrapper.HttpClient);

        NrtEnabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            RequiredHasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            NullableRequiredHasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            HasMany = new ToManyRelationshipModelInRequest
            {
                Data = new List<RelationshipModelIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = RelationshipModelResourceType.RelationshipModels
                    }
                }
            },
            RequiredHasMany = new ToManyRelationshipModelInRequest
            {
                Data = new List<RelationshipModelIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = RelationshipModelResourceType.RelationshipModels
                    }
                }
            }
        };

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new NrtEnabledModelPostRequestDocument
        {
            Data = new NrtEnabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        Func<Task<NrtEnabledModelPrimaryResponseDocument?>> action;
        
        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<NrtEnabledModelPostRequestDocument, NrtEnabledModelRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtEnabledModelAsync(requestDocument));
        }
        
        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Ignored property '{jsonName}' must have a value because it is required. Path 'data.relationships'.");
    }

    [Theory]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.HasOne), "hasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.NullableRequiredHasOne), "nullableRequiredHasOne")]
    [InlineData(nameof(NrtEnabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesEnabledClientRelationshipsObject(wrapper.HttpClient);

        NrtEnabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            RequiredHasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            NullableHasOne = new NullableToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            NullableRequiredHasOne = new ToOneRelationshipModelInRequest
            {
                Data = new RelationshipModelIdentifier
                {
                    Id = "1",
                    Type = RelationshipModelResourceType.RelationshipModels
                }
            },
            HasMany = new ToManyRelationshipModelInRequest
            {
                Data = new List<RelationshipModelIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = RelationshipModelResourceType.RelationshipModels
                    }
                }
            },
            RequiredHasMany = new ToManyRelationshipModelInRequest
            {
                Data = new List<RelationshipModelIdentifier>
                {
                    new()
                    {
                        Id = "1",
                        Type = RelationshipModelResourceType.RelationshipModels
                    }
                }
            }
        };

        relationshipsInPostDocument.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new NrtEnabledModelPostRequestDocument
        {
            Data = new NrtEnabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        // Act
        Func<Task<NrtEnabledModelPrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtEnabledModelAsync(requestDocument));

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
        var apiClient = new NullableReferenceTypesEnabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtEnabledModelPatchRequestDocument()
        {
            Data = new NrtEnabledModelDataInPatchRequest
            {
                Id = "1",
                Type = NrtEnabledModelResourceType.NrtEnabledModels,
                Relationships = new NrtEnabledModelRelationshipsInPatchRequest()
                {
                    NullableHasOne = new NullableToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    HasMany = new ToManyRelationshipModelInRequest
                    {
                        Data = new List<RelationshipModelIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = RelationshipModelResourceType.RelationshipModels
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PatchNrtEnabledModelAsync(1, requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be(NrtEnabledModelUrl + "/1");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtEnabledModels"",
    ""id"": ""1"",
    ""relationships"": {
      ""nullableHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""hasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
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
        var apiClient = new NullableReferenceTypesEnabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtEnabledModelPostRequestDocument()
        {
            Data = new NrtEnabledModelDataInPostRequest
            {
                Relationships = new NrtEnabledModelRelationshipsInPostRequest
                {
                    NullableHasOne = new NullableToOneRelationshipModelInRequest
                    {
                        Data = null
                    },
                    HasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    RequiredHasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    NullableRequiredHasOne = new ToOneRelationshipModelInRequest
                    {
                        Data = new RelationshipModelIdentifier
                        {
                            Id = "1",
                            Type = RelationshipModelResourceType.RelationshipModels
                        }
                    },
                    HasMany = new ToManyRelationshipModelInRequest
                    {
                        Data = new List<RelationshipModelIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = RelationshipModelResourceType.RelationshipModels
                            }
                        }
                    },
                    RequiredHasMany = new ToManyRelationshipModelInRequest
                    {
                        Data = new List<RelationshipModelIdentifier>
                        {
                            new()
                            {
                                Id = "1",
                                Type = RelationshipModelResourceType.RelationshipModels
                            }
                        }
                    }
                }
            }
        };

        await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtEnabledModelAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(NrtEnabledModelUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtEnabledModels"",
    ""relationships"": {
      ""hasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""requiredHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""nullableHasOne"": {
        ""data"": null
      },
      ""nullableRequiredHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      },
      ""hasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
            ""id"": ""1""
          }
        ]
      },
      ""requiredHasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
            ""id"": ""1""
          }
        ]
      }
    }
  }
}");
    }
}
