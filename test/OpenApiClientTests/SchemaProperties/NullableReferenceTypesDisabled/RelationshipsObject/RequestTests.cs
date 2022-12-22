using System.Net;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject;

public sealed class RequestTests
{
    private const string NrtDisabledModelUrl = "http://localhost/nrtDisabledModels";

    [Fact]
    public async Task Can_exclude_optional_relationships()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtDisabledModelPostRequestDocument()
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = new NrtDisabledModelRelationshipsInPostRequest
                {
                    RequiredHasOne = new ToOneRelationshipModelInRequest
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

        await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(NrtDisabledModelUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtDisabledModels"",
    ""relationships"": {
      ""requiredHasOne"": {
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
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        NrtDisabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new NullableToOneRelationshipModelInRequest
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

        var requestDocument = new NrtDisabledModelPostRequestDocument
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        Func<Task<NrtDisabledModelPrimaryResponseDocument?>> action;
        
        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<NrtDisabledModelPostRequestDocument, NrtDisabledModelRelationshipsInPostRequest>(requestDocument))
        {
            // Act
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));
        }
        
        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Ignored property '{jsonName}' must have a value because it is required. Path 'data.relationships'.");
    }

    [Theory]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_exclude_required_relationship_when_performing_POST_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        NrtDisabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new NullableToOneRelationshipModelInRequest
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

        var requestDocument = new NrtDisabledModelPostRequestDocument
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        // Act
        Func<Task<NrtDisabledModelPrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));

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
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtDisabledModelPatchRequestDocument()
        {
            Data = new NrtDisabledModelDataInPatchRequest
            {
                Id = "1",
                Type = NrtDisabledModelResourceType.NrtDisabledModels,
                Relationships = new NrtDisabledModelRelationshipsInPatchRequest()
                {
                    HasOne = new NullableToOneRelationshipModelInRequest
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

        await ApiResponse.TranslateAsync(async () => await apiClient.PatchNrtDisabledModelAsync(1, requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be(NrtDisabledModelUrl + "/1");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtDisabledModels"",
    ""id"": ""1"",
    ""relationships"": {
      ""hasOne"": {
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
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        var requestDocument = new NrtDisabledModelPostRequestDocument()
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = new NrtDisabledModelRelationshipsInPostRequest
                {
                    HasOne = new NullableToOneRelationshipModelInRequest
                    {
                        Data = null
                    },
                    RequiredHasOne = new ToOneRelationshipModelInRequest
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

        await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));

        // Assert
        wrapper.Request.ShouldNotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be(NrtDisabledModelUrl);
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtDisabledModels"",
    ""relationships"": {
      ""hasOne"": {
        ""data"": null
      },
      ""requiredHasOne"": {
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

    [Theory]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.HasMany), "hasMany")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_clear_non_nullable_relationships_with_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        NrtDisabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new NullableToOneRelationshipModelInRequest
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

        PropertyInfo relationshipToClearPropertyInfo = relationshipsInPostDocument.GetType().GetProperties().Single(property => property.Name == propertyName);
        object relationshipToClear = relationshipToClearPropertyInfo.GetValue(relationshipsInPostDocument)!;
        relationshipToClear.SetPropertyToDefaultValue("Data");

        var requestDocument = new NrtDisabledModelPostRequestDocument
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };

        Func<Task<NrtDisabledModelPrimaryResponseDocument?>> action;
        
        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<NrtDisabledModelPostRequestDocument, NrtDisabledModelRelationshipsInPostRequest>(requestDocument, model => model.RequiredHasOne, model => model.HasMany, model => model.RequiredHasMany))
        {
            // Act
            action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));
        }
        
        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonName}'.");
    }
    
    [Theory]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne), "requiredHasOne")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.HasMany), "hasMany")]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany), "requiredHasMany")]
    public async Task Cannot_clear_non_nullable_relationships_without_document_registration(string propertyName, string jsonName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        NrtDisabledModelRelationshipsInPostRequest relationshipsInPostDocument = new()
        {
            HasOne = new NullableToOneRelationshipModelInRequest
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

        PropertyInfo relationshipToClearPropertyInfo = relationshipsInPostDocument.GetType().GetProperties().Single(property => property.Name == propertyName);
        object relationshipToClear = relationshipToClearPropertyInfo.GetValue(relationshipsInPostDocument)!;
        relationshipToClear.SetPropertyToDefaultValue("Data");

        var requestDocument = new NrtDisabledModelPostRequestDocument
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = relationshipsInPostDocument
            }
        };
        
        // Act
        Func<Task<NrtDisabledModelPrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should().Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonName}'.");
    }
}
