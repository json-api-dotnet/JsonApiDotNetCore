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

public sealed class RequestTestsAlt
{
    private const string NrtDisabledModelUrl = "http://localhost/nrtDisabledModels";

    private readonly Dictionary<string, string> _partials = new()
    {
        {
            nameof(NrtDisabledModelRelationshipsInPostRequest.HasOne), @"""hasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      }"
        },
        {
            nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne), @"""requiredHasOne"": {
        ""data"": {
          ""type"": ""relationshipModels"",
          ""id"": ""1""
        }
      }"
        },

        {
            nameof(NrtDisabledModelRelationshipsInPostRequest.HasMany), @"""hasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
            ""id"": ""1""
          }
        ]
      }"
        },

        {
            nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany), @"""requiredHasMany"": {
        ""data"": [
          {
            ""type"": ""relationshipModels"",
            ""id"": ""1""
          }
        ]
      }"
        }
    };

    [Theory]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.HasOne))]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.HasMany))]
    public async Task Can_exclude_optional_relationships(string propertyName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        NrtDisabledModelRelationshipsInPostRequest relationshipsObject = new()
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

        relationshipsObject.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new NrtDisabledModelPostRequestDocument
        {
            Data = new NrtDisabledModelDataInPostRequest
            {
                Relationships = relationshipsObject
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

        string body = GetRelationshipsObjectWithSinglePropertyOmitted(propertyName);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtDisabledModels"",
    ""relationships"": "+ body +@"
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

    [Theory]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasOne))]
    [InlineData(nameof(NrtDisabledModelRelationshipsInPostRequest.RequiredHasMany))]
    public async Task Can_exclude_relationships_that_are_required_for_POST_when_performing_PATCH(string propertyName)
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NullableReferenceTypesDisabledClientRelationshipsObject(wrapper.HttpClient);

        var relationshipsObject = new NrtDisabledModelRelationshipsInPatchRequest()
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

        relationshipsObject.SetPropertyToDefaultValue(propertyName);

        var requestDocument = new NrtDisabledModelPatchRequestDocument()
        {
            Data = new NrtDisabledModelDataInPatchRequest
            {
                Id = "1",
                Type = NrtDisabledModelResourceType.NrtDisabledModels,
                Relationships = relationshipsObject
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

        string serializedRelationshipsObject = GetRelationshipsObjectWithSinglePropertyOmitted(propertyName);

        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""nrtDisabledModels"",
    ""id"": ""1"",
    ""relationships"": "+ serializedRelationshipsObject +@"
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

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<NrtDisabledModelPostRequestDocument, NrtDisabledModelRelationshipsInPostRequest>(requestDocument, model => model.RequiredHasOne, model => model.HasMany, model => model.RequiredHasMany))
        {
            // Act
            Func<Task<NrtDisabledModelPrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostNrtDisabledModelAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonName}'.");
        }
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
