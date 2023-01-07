using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn;

public sealed class CreateResourceTests : OpenApiClientTests
{
    private readonly NrtOnMsvOnFakers _fakers = new();

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), "nullableValueType")]
    public async Task Can_clear_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateIncludedAttributeSelector<ResourceAttributesInPostRequest>(attributePropertyName);

        using (apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath(jsonPropertyName).With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.Null));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Can_set_default_value_to_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? defaultValue = requestDocument.Data.Attributes.GetDefaultValueForProperty(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateIncludedAttributeSelector<ResourceAttributesInPostRequest>(attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldContainPath(jsonPropertyName).With(attribute => attribute.ShouldBeInteger(0));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    public async Task Cannot_clear_attribute(string clrPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        requestDocument.Data.Attributes.SetPropertyValue(clrPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateIncludedAttributeSelector<ResourceAttributesInPostRequest>(clrPropertyName);

        using (apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should().Contain($"Cannot write a null value for property '{jsonPropertyName}'.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableReferenceType), "nullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), "nullableValueType")]
    public async Task Can_exclude_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceAttributesInPostRequest emptyAttributesObject = new();
        object? defaultValue = emptyAttributesObject.GetPropertyValue(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.ShouldNotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.NonNullableReferenceType), "nonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNonNullableReferenceType), "requiredNonNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableReferenceType), "requiredNullableReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), "requiredNullableValueType")]
    public async Task Cannot_exclude_attribute(string attributePropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceAttributesInPostRequest emptyAttributesObject = new();
        object? defaultValue = emptyAttributesObject.GetPropertyValue(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

            // Assert
            ExceptionAssertions<InvalidOperationException> assertion = await action.Should().ThrowExactlyAsync<InvalidOperationException>();
            InvalidOperationException exception = assertion.Subject.Single();
            exception.Message.Should().Be($"The following property should not be omitted: data.attributes.{jsonPropertyName}.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), nameof(ResourceRelationshipsInPostRequest.NullableToOne.Data), "nullableToOne")]
    public async Task Can_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string dataPropertyName,
        string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(dataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), nameof(ResourceRelationshipsInPostRequest.NullableToOne.Data), "nullableToOne")]
    public async Task Can_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string dataPropertyName,
        string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(dataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), nameof(ResourceRelationshipsInPostRequest.NonNullableToOne.Data),
        "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne.Data),
        "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne.Data),
        "requiredNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), nameof(ResourceRelationshipsInPostRequest.ToMany.Data), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), nameof(ResourceRelationshipsInPostRequest.RequiredToMany.Data), "requiredToMany")]
    public async Task Cannot_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string dataPropertyName,
        string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(dataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should()
                .Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), nameof(ResourceRelationshipsInPostRequest.NonNullableToOne.Data),
        "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne.Data),
        "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne.Data),
        "requiredNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), nameof(ResourceRelationshipsInPostRequest.ToMany.Data), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), nameof(ResourceRelationshipsInPostRequest.RequiredToMany.Data), "requiredToMany")]
    public async Task Cannot_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string dataPropertyName,
        string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(dataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should()
            .Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_exclude_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NullableToOne), "nullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Can_exclude_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.ParseRequestBody();

        document.ShouldContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.ShouldNotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), "requiredNullableToOne")]
    public async Task Cannot_exclude_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
                await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should()
                .Be($"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.NonNullableToOne), "nonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNonNullableToOne), "requiredNonNullableToOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredNullableToOne), "requiredNullableToOne")]
    public async Task Cannot_exclude_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    NonNullableToOne = _fakers.ToOne.Generate(),
                    RequiredNonNullableToOne = _fakers.ToOne.Generate(),
                    NullableToOne = _fakers.NullableToOne.Generate(),
                    RequiredNullableToOne = _fakers.ToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () =>
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should()
            .Be($"Cannot write a null value for property 'id'. Property requires a value. Path 'data.relationships.{jsonPropertyName}.data'.");
    }
}
