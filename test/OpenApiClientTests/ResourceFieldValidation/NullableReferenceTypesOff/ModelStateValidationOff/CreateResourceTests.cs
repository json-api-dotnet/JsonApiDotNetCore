using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Specialized;
using Newtonsoft.Json;
using OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff;

public sealed class CreateResourceTests : OpenApiClientTests
{
    private const string DataPropertyName = "Data";
    private readonly NrtOffMsvOffFakers _fakers = new();

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), "requiredReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.NullableValueType), "nullableValueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredNullableValueType), "requiredNullableValueType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using (apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.ValueKind.Should().Be(JsonValueKind.Null));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? defaultValue = requestDocument.Data.Attributes.GetDefaultValueForProperty(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        Expression<Func<ResourceAttributesInPostRequest, object?>> includeAttributeSelector =
            CreateAttributeSelectorFor<ResourceAttributesInPostRequest>(attributePropertyName);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization(requestDocument, includeAttributeSelector))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().ContainPath(jsonPropertyName).With(attribute => attribute.Should().Be(0));
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.ReferenceType), "referenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.ValueType), "valueType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceAttributesInPostRequest emptyAttributesObject = new();
        object? defaultValue = emptyAttributesObject.GetPropertyValue(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.attributes").With(attributesObject =>
        {
            attributesObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredReferenceType), "requiredReferenceType")]
    [InlineData(nameof(ResourceAttributesInPostRequest.RequiredValueType), "requiredValueType")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceAttributesInPostRequest emptyAttributesObject = new();
        object? defaultValue = emptyAttributesObject.GetPropertyValue(attributePropertyName);
        requestDocument.Data.Attributes.SetPropertyValue(attributePropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

            // Assert
            ExceptionAssertions<InvalidOperationException> assertion = await action.Should().ThrowExactlyAsync<InvalidOperationException>();
            InvalidOperationException exception = assertion.Subject.Single();

            exception.Message.Should().Be(
                $"Required property '{attributePropertyName}' at JSON path 'data.attributes.{jsonPropertyName}' is not set. If sending its default value is intended, include it explicitly.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToOne), "requiredToOne")]
    public async Task Can_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(DataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToOne), "requiredToOne")]
    public async Task Can_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(DataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        // Act
        await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath($"data.relationships.{jsonPropertyName}.data").With(relationshipDataObject =>
        {
            relationshipDataObject.ValueKind.Should().Be(JsonValueKind.Null);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship_with_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(DataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

            // Assert
            ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
            JsonSerializationException exception = assertion.Subject.Single();

            exception.Message.Should()
                .Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
        }
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.RequiredToMany), "requiredToMany")]
    public async Task Cannot_clear_relationship_without_partial_attribute_serialization(string relationshipPropertyName, string jsonPropertyName)
    {
        // Arrange
        var requestDocument = new ResourcePostRequestDocument
        {
            Data = new ResourceDataInPostRequest
            {
                Attributes = _fakers.PostAttributes.Generate(),
                Relationships = new ResourceRelationshipsInPostRequest
                {
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        object? relationshipObject = requestDocument.Data.Relationships.GetPropertyValue(relationshipPropertyName);
        relationshipObject!.SetPropertyValue(DataPropertyName, null);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        // Act
        Func<Task<ResourcePrimaryResponseDocument?>> action = async () => await apiClient.PostResourceAsync(requestDocument);

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        JsonSerializationException exception = assertion.Subject.Single();

        exception.Message.Should()
            .Be($"Cannot write a null value for property 'data'. Property requires a value. Path 'data.relationships.{jsonPropertyName}'.");
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }

    [Theory]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToOne), "toOne")]
    [InlineData(nameof(ResourceRelationshipsInPostRequest.ToMany), "toMany")]
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
                    ToOne = _fakers.NullableToOne.Generate(),
                    RequiredToOne = _fakers.NullableToOne.Generate(),
                    ToMany = _fakers.ToMany.Generate(),
                    RequiredToMany = _fakers.ToMany.Generate()
                }
            }
        };

        ResourceRelationshipsInPostRequest emptyRelationshipsObject = new();
        object? defaultValue = emptyRelationshipsObject.GetPropertyValue(relationshipPropertyName);
        requestDocument.Data.Relationships.SetPropertyValue(relationshipPropertyName, defaultValue);

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOffMsvOffClient(wrapper.HttpClient);

        using (apiClient.WithPartialAttributeSerialization<ResourcePostRequestDocument, ResourceAttributesInPostRequest>(requestDocument))
        {
            // Act
            await ApiResponse.TranslateAsync(async () => await apiClient.PostResourceAsync(requestDocument));
        }

        // Assert
        JsonElement document = wrapper.GetRequestBodyAsJson();

        document.Should().ContainPath("data.relationships").With(relationshipsObject =>
        {
            relationshipsObject.Should().NotContainPath(jsonPropertyName);
        });
    }
}
