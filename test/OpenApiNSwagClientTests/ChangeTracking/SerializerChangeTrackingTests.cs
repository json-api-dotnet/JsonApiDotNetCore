using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagClientTests.NamingConventions.CamelCase.GeneratedCode;
using OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOn.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.ChangeTracking;

public sealed class SerializerChangeTrackingTests
{
    [Fact]
    public async Task Includes_properties_with_default_values_when_tracked()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0,
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        requestBody.Data.Attributes.RequiredNonNullableReferenceType = "other";

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "requiredNonNullableReferenceType": "other",
                  "nullableReferenceType": null,
                  "valueType": 0,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Excludes_properties_with_default_values_when_not_tracked()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient).Initializer
            }
        };

        requestBody.Data.Attributes.RequiredNonNullableReferenceType = "other";

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "requiredNonNullableReferenceType": "other"
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Properties_can_be_changed_to_default_values_once_tracked()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 1,
                        NullableValueType = 2,
                        NullableReferenceType = "other"
                    }
                }.Initializer
            }
        };

        requestBody.Data.Attributes.ValueType = 0;
        requestBody.Data.Attributes.NullableValueType = null;
        requestBody.Data.Attributes.NullableReferenceType = null;

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "valueType": 0,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Automatically_clears_tracked_properties_after_sending_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0,
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));
        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        requestBody.Data.Attributes.ValueType = 1;
        requestBody.Data.Attributes.RequiredValueType = 2;
        requestBody.Data.Attributes.RequiredNullableValueType = 3;

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "valueType": 1,
                  "requiredValueType": 2,
                  "requiredNullableValueType": 3
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_preserve_tracked_properties_after_sending_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);

        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient)
        {
            AutoClearTracked = false
        };

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0,
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));
        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "valueType": 0,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_manually_clear_tracked_properties()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);

        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient)
        {
            AutoClearTracked = false
        };

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0,
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        apiClient.ClearAllTracked();

        requestBody.Data.Attributes.ValueType = 1;
        requestBody.Data.Attributes.RequiredValueType = 2;
        requestBody.Data.Attributes.RequiredNullableValueType = 3;

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "valueType": 1,
                  "requiredValueType": 2,
                  "requiredNullableValueType": 3
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_mark_existing_instance_as_tracked()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new AttributesInUpdateResourceRequest()
            }
        };

        apiClient.MarkAsTracked(requestBody.Data.Attributes);
        requestBody.Data.Attributes.RequiredNonNullableReferenceType = "other";

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "requiredNonNullableReferenceType": "other"
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_mark_properties_on_existing_instance_as_tracked()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new AttributesInUpdateResourceRequest()
            }
        };

        string[] propertyNamesToTrack =
        [
            nameof(AttributesInUpdateResourceRequest.ValueType),
            nameof(AttributesInUpdateResourceRequest.NullableValueType),
            nameof(AttributesInUpdateResourceRequest.NullableReferenceType)
        ];

        apiClient.MarkAsTracked(requestBody.Data.Attributes, propertyNamesToTrack);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "valueType": 0,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public void Can_recursively_track_properties_on_complex_object()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new ExposeSerializerSettingsOnApiClient(wrapper.HttpClient);

        ComplexType complexObject = new TrackChangesFor<ComplexType>(apiClient)
        {
            Initializer =
            {
                NullableDateTime = null,
                NestedType = new TrackChangesFor<NestedType>(apiClient)
                {
                    Initializer =
                    {
                        NullableInt = null,
                        NullableString = null
                    }
                }.Initializer
            }
        }.Initializer;

        JsonSerializerSettings serializerSettings = apiClient.GetSerializerSettings();

        // Act
        string json = JsonConvert.SerializeObject(complexObject, serializerSettings);

        // Assert
        json.Should().BeJson("""
            {
              "NullableDateTime": null,
              "NestedType": {
                "NullableInt": null,
                "NullableString": null
              }
            }
            """);
    }

    [Fact]
    public async Task Tracking_a_different_instance_of_same_type_upfront_is_isolated()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        _ = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0
                    }
                }.Initializer
            }
        };

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Tracking_a_different_instance_of_same_type_afterward_is_isolated()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        _ = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0
                    }
                }.Initializer
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_reuse_api_client()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody1 = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0
                    }
                }.Initializer
            }
        };

        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody1));
        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        var requestBody2 = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        NullableValueType = null,
                        NullableReferenceType = null
                    }
                }.Initializer
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody2));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "nullableReferenceType": null,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_reuse_request_document_on_same_api_client()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new NrtOnMsvOnClient(wrapper.HttpClient);

        string resourceId = Unknown.StringId.Int32;

        var requestBody = new UpdateResourceRequestDocument
        {
            Data = new DataInUpdateResourceRequest
            {
                Id = resourceId,
                Attributes = new TrackChangesFor<AttributesInUpdateResourceRequest>(apiClient)
                {
                    Initializer =
                    {
                        ValueType = 0,
                        RequiredNonNullableReferenceType = "first"
                    }
                }.Initializer
            }
        };

        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));
        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        requestBody.Data.Attributes.NullableValueType = null;
        requestBody.Data.Attributes.NullableReferenceType = null;
        requestBody.Data.Attributes.RequiredNonNullableReferenceType = "other";

        string[] propertyNamesToTrack =
        [
            nameof(AttributesInUpdateResourceRequest.NullableValueType),
            nameof(AttributesInUpdateResourceRequest.NullableReferenceType)
        ];

        apiClient.MarkAsTracked(requestBody.Data.Attributes, propertyNamesToTrack);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchResourceAsync(resourceId, null, requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "resources",
                "id": "{{resourceId}}",
                "attributes": {
                  "openapi:discriminator": "resources",
                  "requiredNonNullableReferenceType": "other",
                  "nullableReferenceType": null,
                  "nullableValueType": null
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Can_track_multiple_times_in_same_request_document()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new CamelCaseClient(wrapper.HttpClient);

        var requestBody = new OperationsRequestDocument
        {
            Atomic_operations =
            [
                new UpdateStaffMemberOperation
                {
                    Data = new DataInUpdateStaffMemberRequest
                    {
                        Attributes = new TrackChangesFor<AttributesInUpdateStaffMemberRequest>(apiClient)
                        {
                            Initializer =
                            {
                                Age = null
                            }
                        }.Initializer
                    }
                },
                new UpdateStaffMemberOperation
                {
                    Data = new DataInUpdateStaffMemberRequest
                    {
                        Attributes = new TrackChangesFor<AttributesInUpdateStaffMemberRequest>(apiClient)
                        {
                            Initializer =
                            {
                                Name = "new-name"
                            }
                        }.Initializer
                    }
                },
                new UpdateSupermarketOperation
                {
                    Data = new DataInUpdateSupermarketRequest
                    {
                        Attributes = new TrackChangesFor<AttributesInUpdateSupermarketRequest>(apiClient)
                        {
                            Initializer =
                            {
                                NameOfCity = "new-name-of-city"
                            }
                        }.Initializer
                    }
                },
                new UpdateSupermarketOperation
                {
                    Data = new DataInUpdateSupermarketRequest
                    {
                        Attributes = new TrackChangesFor<AttributesInUpdateSupermarketRequest>(apiClient)
                        {
                            Initializer =
                            {
                                Kind = null
                            }
                        }.Initializer
                    }
                }
            ]
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        wrapper.RequestBody.Should().BeJson("""
            {
              "atomic:operations": [
                {
                  "openapi:discriminator": "updateStaffMemberOperation",
                  "op": "update",
                  "data": {
                    "type": "staffMembers",
                    "attributes": {
                      "openapi:discriminator": "staffMembers",
                      "age": null
                    }
                  }
                },
                {
                  "openapi:discriminator": "updateStaffMemberOperation",
                  "op": "update",
                  "data": {
                    "type": "staffMembers",
                    "attributes": {
                      "openapi:discriminator": "staffMembers",
                      "name": "new-name"
                    }
                  }
                },
                {
                  "openapi:discriminator": "updateSupermarketOperation",
                  "op": "update",
                  "data": {
                    "type": "supermarkets",
                    "attributes": {
                      "openapi:discriminator": "supermarkets",
                      "nameOfCity": "new-name-of-city"
                    }
                  }
                },
                {
                  "openapi:discriminator": "updateSupermarketOperation",
                  "op": "update",
                  "data": {
                    "type": "supermarkets",
                    "attributes": {
                      "openapi:discriminator": "supermarkets",
                      "kind": null
                    }
                  }
                }
              ]
            }
            """);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ComplexType : NotifyPropertySet
    {
        private DateTime? _nullableDateTime;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? NullableDateTime
        {
            get => _nullableDateTime;
            set => SetProperty(ref _nullableDateTime, value);
        }

        public NestedType? NestedType { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class NestedType : NotifyPropertySet
    {
        private int? _nullableInt;
        private string? _nullableString;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? NullableInt
        {
            get => _nullableInt;
            set => SetProperty(ref _nullableInt, value);
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? NullableString
        {
            get => _nullableString;
            set => SetProperty(ref _nullableString, value);
        }
    }

    private sealed class ExposeSerializerSettingsOnApiClient(HttpClient httpClient)
        : NrtOnMsvOnClient(httpClient)
    {
        public JsonSerializerSettings GetSerializerSettings()
        {
            return JsonSerializerSettings;
        }
    }
}
