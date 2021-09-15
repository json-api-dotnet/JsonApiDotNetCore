using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using OpenApiClientTests.LegacyClient.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

#pragma warning disable AV1704 // Don't include numbers in variables, parameters and type members

namespace OpenApiClientTests.LegacyClient
{
    public sealed class ClientAttributeRegistrationLifetimeTests
    {
        [Fact]
        public async Task Disposed_attribute_registration_for_document_does_not_affect_request()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            const string airplaneId = "XUuiP";
            var manufacturedAt = 1.January(2021).At(15, 23, 5, 33).ToDateTimeOffset(4.Hours());

            var requestDocument = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        ManufacturedAt = manufacturedAt
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
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
      ""manufactured-at"": ""2021-01-01T15:23:05.033+04:00"",
      ""is-in-maintenance"": false
    }
  }
}");
        }

        [Fact]
        public async Task Attribute_registration_can_be_used_for_multiple_requests()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            const string airplaneId = "XUuiP";

            var requestDocument = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        AirtimeInHours = 100
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
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
        public async Task Request_is_unaffected_by_attribute_registration_for_different_document_of_same_type()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            const string airplaneId1 = "XUuiP";
            var manufacturedAt = 1.January(2021).At(15, 23, 5, 33).ToDateTimeOffset(4.Hours());

            var requestDocument1 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId1,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest()
                }
            };

            const string airplaneId2 = "DJy1u";

            var requestDocument2 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId2,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        ManufacturedAt = manufacturedAt
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
                airplane => airplane.AirtimeInHours))
            {
                using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
                    airplane => airplane.ManufacturedAt))
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
      ""manufactured-at"": ""2021-01-01T15:23:05.033+04:00"",
      ""is-in-maintenance"": false
    }
  }
}");
        }

        [Fact]
        public async Task Attribute_values_can_be_changed_after_attribute_registration()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            const string airplaneId = "XUuiP";

            var requestDocument = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        IsInMaintenance = true
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
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
        public async Task Attribute_registration_is_unaffected_by_successive_attribute_registration_for_document_of_different_type()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            const string airplaneId1 = "XUuiP";

            var requestDocument1 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId1,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest()
                }
            };

            var requestDocument2 = new AirplanePostRequestDocument
            {
                Data = new AirplaneDataInPostRequest
                {
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPostRequest()
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
                airplane => airplane.IsInMaintenance))
            {
                using (apiClient.RegisterAttributesForRequestDocument<AirplanePostRequestDocument, AirplaneAttributesInPostRequest>(requestDocument2,
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
        public async Task Attribute_registration_is_unaffected_by_preceding_disposed_attribute_registration_for_different_document_of_same_type()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            const string airplaneId1 = "XUuiP";

            var requestDocument1 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId1,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest()
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
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
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        SerialNumber = "100"
                    }
                }
            };

            wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
                airplane => airplane.LastServicedAt))
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
      ""last-serviced-at"": null,
      ""serial-number"": ""100""
    }
  }
}");
        }

        [Fact]
        public async Task Attribute_registration_is_unaffected_by_preceding_disposed_attribute_registration_for_document_of_different_type()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            var requestDocument1 = new AirplanePostRequestDocument
            {
                Data = new AirplaneDataInPostRequest
                {
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPostRequest()
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePostRequestDocument, AirplaneAttributesInPostRequest>(requestDocument1,
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
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        SerialNumber = "100"
                    }
                }
            };

            wrapper.ChangeResponse(HttpStatusCode.NoContent, null);

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
                airplane => airplane.LastServicedAt))
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
      ""last-serviced-at"": null,
      ""serial-number"": ""100""
    }
  }
}");
        }

        [Fact]
        public async Task Attribute_registration_is_unaffected_by_preceding_attribute_registration_for_different_document_of_same_type()
        {
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            ILegacyClient apiClient = new GeneratedCode.LegacyClient(wrapper.HttpClient);

            // Arrange
            const string airplaneId1 = "XUuiP";

            var requestDocument1 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId1,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest()
                }
            };

            const string airplaneId2 = "DJy1u";

            var requestDocument2 = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId2,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest()
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument1,
                airplane => airplane.ManufacturedAt))
            {
                using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument2,
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
}
