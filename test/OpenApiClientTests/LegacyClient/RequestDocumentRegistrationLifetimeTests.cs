using System.Net;
using FluentAssertions;
using OpenApiClientTests.LegacyClient.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.LegacyClient;

public sealed class RequestDocumentRegistrationLifetimeTests
{
    [Fact]
    public async Task Disposed_request_document_registration_does_not_affect_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId = "XUuiP";

        var requestDocument = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
            airplane => airplane.AirtimeInHours))
        {
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));
        }

        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId + @""",
    ""attributes"": {
      ""is-in-maintenance"": false
    }
  }
}");
    }

    [Fact]
    public async Task Request_document_registration_can_be_used_for_multiple_requests()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId = "XUuiP";

        var requestDocument = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest
                {
                    AirtimeInHours = 100
                }
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
            airplane => airplane.AirtimeInHours))
        {
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));

            wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

            requestDocument.Data.Attributes.AirtimeInHours = null;

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId + @""",
    ""attributes"": {
      ""airtime-in-hours"": null
    }
  }
}");
    }

    [Fact]
    public async Task Request_is_unaffected_by_request_document_registration_of_different_request_document_of_same_type()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId1 = "XUuiP";

        var requestDocument1 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId1,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        const string airplaneId2 = "DJy1u";

        var requestDocument2 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId2,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
            airplane => airplane.AirtimeInHours))
        {
            using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
                airplane => airplane.SerialNumber))
            {
            }

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId2, requestDocument2));
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId2 + @""",
    ""attributes"": {
      ""is-in-maintenance"": false
    }
  }
}");
    }

    [Fact]
    public async Task Attribute_values_can_be_changed_after_request_document_registration()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId = "XUuiP";

        var requestDocument = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest
                {
                    IsInMaintenance = true
                }
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
            airplane => airplane.IsInMaintenance))
        {
            requestDocument.Data.Attributes.IsInMaintenance = false;

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId + @""",
    ""attributes"": {
      ""is-in-maintenance"": false
    }
  }
}");
    }

    [Fact]
    public async Task Request_document_registration_is_unaffected_by_successive_registration_of_request_document_of_different_type()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId1 = "XUuiP";

        var requestDocument1 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId1,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        var requestDocument2 = new AirplanePostRequestDocument
        {
            Data = new AirplaneDataInPostRequest
            {
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPostRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
            airplane => airplane.IsInMaintenance))
        {
            using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePostRequestDocument, AirplaneAttributesInPostRequest>(requestDocument2,
                airplane => airplane.AirtimeInHours))
            {
                // Act
                _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId1, requestDocument1));
            }
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId1 + @""",
    ""attributes"": {
      ""is-in-maintenance"": false
    }
  }
}");
    }

    [Fact]
    public async Task Request_document_registration_is_unaffected_by_preceding_disposed_registration_of_different_request_document_of_same_type()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId1 = "XUuiP";

        var requestDocument1 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId1,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
            airplane => airplane.AirtimeInHours))
        {
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId1, requestDocument1));
        }

        const string airplaneId2 = "DJy1u";

        var requestDocument2 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId2,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest
                {
                    ManufacturedInCity = "Everett"
                }
            }
        };

        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
            airplane => airplane.SerialNumber))
        {
            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId2, requestDocument2));
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId2 + @""",
    ""attributes"": {
      ""serial-number"": null,
      ""manufactured-in-city"": ""Everett""
    }
  }
}");
    }

    [Fact]
    public async Task Request_document_registration_is_unaffected_by_preceding_disposed_registration_of_request_document_of_different_type()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        var requestDocument1 = new AirplanePostRequestDocument
        {
            Data = new AirplaneDataInPostRequest
            {
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPostRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePostRequestDocument, AirplaneAttributesInPostRequest>(requestDocument1,
            airplane => airplane.AirtimeInHours))
        {
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostAirplaneAsync(requestDocument1));
        }

        const string airplaneId = "DJy1u";

        var requestDocument2 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest
                {
                    ManufacturedInCity = "Everett"
                }
            }
        };

        wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
            airplane => airplane.SerialNumber))
        {
            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument2));
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId + @""",
    ""attributes"": {
      ""serial-number"": null,
      ""manufactured-in-city"": ""Everett""
    }
  }
}");
    }

    [Fact]
    public async Task Request_document_registration_is_unaffected_by_preceding_registration_of_different_request_document_of_same_type()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

        const string airplaneId1 = "XUuiP";

        var requestDocument1 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId1,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        const string airplaneId2 = "DJy1u";

        var requestDocument2 = new AirplanePatchRequestDocument
        {
            Data = new AirplaneDataInPatchRequest
            {
                Id = airplaneId2,
                Type = AirplaneResourceType.Airplanes,
                Attributes = new AirplaneAttributesInPatchRequest()
            }
        };

        using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
            airplane => airplane.SerialNumber))
        {
            using (apiClient.OmitDefaultValuesForAttributesInRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
                airplane => airplane.IsInMaintenance, airplane => airplane.AirtimeInHours))
            {
                // Act
                _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId2, requestDocument2));
            }
        }

        // Assert
        wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + airplaneId2 + @""",
    ""attributes"": {
      ""airtime-in-hours"": null,
      ""is-in-maintenance"": false
    }
  }
}");
    }
}
