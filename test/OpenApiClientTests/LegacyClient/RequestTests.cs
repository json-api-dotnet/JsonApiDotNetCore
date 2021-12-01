using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Middleware;
using Microsoft.Net.Http.Headers;
using OpenApiClientTests.LegacyClient.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiClientTests.LegacyClient
{
    public sealed class RequestTests
    {
        private const string HostPrefix = "http://localhost/api/v1/";

        [Fact]
        public async Task Getting_resource_collection_produces_expected_request()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCollectionAsync());

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + "flights");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Getting_resource_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightAsync(flightId));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Partial_posting_resource_with_selected_relationships_produces_expected_request()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new FlightPostRequestDocument
            {
                Data = new FlightDataInPostRequest
                {
                    Type = FlightsResourceType.Flights,
                    Relationships = new FlightRelationshipsInPostRequest
                    {
                        Purser = new ToOneFlightAttendantInRequest
                        {
                            Data = new FlightAttendantIdentifier
                            {
                                Id = "bBJHu",
                                Type = FlightAttendantsResourceType.FlightAttendants
                            }
                        },
                        BackupPurser = new NullableToOneFlightAttendantInRequest
                        {
                            Data = new FlightAttendantIdentifier
                            {
                                Id = "NInmX",
                                Type = FlightAttendantsResourceType.FlightAttendants
                            }
                        }
                    }
                }
            };

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostFlightAsync(requestDocument));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Post);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + "flights");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""flights"",
    ""relationships"": {
      ""purser"": {
        ""data"": {
          ""type"": ""flight-attendants"",
          ""id"": ""bBJHu""
        }
      },
      ""backup-purser"": {
        ""data"": {
          ""type"": ""flight-attendants"",
          ""id"": ""NInmX""
        }
      }
    }
  }
}");
        }

        [Fact]
        public async Task Partial_posting_resource_produces_expected_request()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            const char euroSign = '\x20AC';
            const char checkMark = '\x2713';
            const char capitalLWithStroke = '\x0141';

            string specialCharacters = new(new[]
            {
                euroSign,
                checkMark,
                capitalLWithStroke
            });

            string name = "anAirplaneName " + specialCharacters;

            var requestDocument = new AirplanePostRequestDocument
            {
                Data = new AirplaneDataInPostRequest
                {
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPostRequest
                    {
                        Name = name,
                        AirtimeInHours = 800
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePostRequestDocument, AirplaneAttributesInPostRequest>(requestDocument,
                airplane => airplane.SerialNumber))
            {
                // Act
                _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostAirplaneAsync(requestDocument));
            }

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Post);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + "airplanes");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""attributes"": {
      ""name"": """ + name + @""",
      ""serial-number"": null,
      ""airtime-in-hours"": 800
    }
  }
}");
        }

        [Fact]
        public async Task Partial_patching_resource_produces_expected_request()
        {
            // Arrange
            const string airplaneId = "XUuiP";
            var lastServicedAt = 1.January(2021).At(15, 23, 5, 33).ToDateTimeOffset(4.Hours());

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new AirplanePatchRequestDocument
            {
                Data = new AirplaneDataInPatchRequest
                {
                    Id = airplaneId,
                    Type = AirplanesResourceType.Airplanes,
                    Attributes = new AirplaneAttributesInPatchRequest
                    {
                        LastServicedAt = lastServicedAt
                    }
                }
            };

            using (apiClient.RegisterAttributesForRequestDocument<AirplanePatchRequestDocument, AirplaneAttributesInPatchRequest>(requestDocument,
                airplane => airplane.SerialNumber, airplane => airplane.LastServicedAt, airplane => airplane.IsInMaintenance,
                airplane => airplane.AirtimeInHours))
            {
                // Act
                _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, requestDocument));
            }

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Patch);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"airplanes/{airplaneId}");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": ""XUuiP"",
    ""attributes"": {
      ""serial-number"": null,
      ""airtime-in-hours"": null,
      ""last-serviced-at"": ""2021-01-01T15:23:05.033+04:00"",
      ""is-in-maintenance"": false
    }
  }
}");
        }

        [Fact]
        public async Task Deleting_resource_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            await apiClient.DeleteFlightAsync(flightId);

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Method.Should().Be(HttpMethod.Delete);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Getting_secondary_resource_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightPurserAsync(flightId));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/purser");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Getting_secondary_resources_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCabinCrewMembersAsync(flightId));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/cabin-crew-members");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Getting_ToOne_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightPurserRelationshipAsync(flightId));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/purser");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Patching_ToOne_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new ToOneFlightAttendantInRequest
            {
                Data = new FlightAttendantIdentifier
                {
                    Id = "bBJHu",
                    Type = FlightAttendantsResourceType.FlightAttendants
                }
            };

            // Act
            await apiClient.PatchFlightPurserRelationshipAsync(flightId, requestDocument);

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Method.Should().Be(HttpMethod.Patch);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/purser");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": {
    ""type"": ""flight-attendants"",
    ""id"": ""bBJHu""
  }
}");
        }

        [Fact]
        public async Task Getting_ToMany_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCabinCrewMembersRelationshipAsync(flightId));

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(HeaderConstants.MediaType);
            wrapper.Request.Method.Should().Be(HttpMethod.Get);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/cabin-crew-members");
            wrapper.RequestBody.Should().BeNull();
        }

        [Fact]
        public async Task Posting_ToMany_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Type = FlightAttendantsResourceType.FlightAttendants,
                        Id = "bBJHu"
                    },
                    new()
                    {
                        Type = FlightAttendantsResourceType.FlightAttendants,
                        Id = "NInmX"
                    }
                }
            };

            // Act
            await apiClient.PostFlightCabinCrewMembersRelationshipAsync(flightId, requestDocument);

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Method.Should().Be(HttpMethod.Post);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/cabin-crew-members");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": [
    {
      ""type"": ""flight-attendants"",
      ""id"": ""bBJHu""
    },
    {
      ""type"": ""flight-attendants"",
      ""id"": ""NInmX""
    }
  ]
}");
        }

        [Fact]
        public async Task Patching_ToMany_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Id = "bBJHu",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    },
                    new()
                    {
                        Id = "NInmX",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    }
                }
            };

            // Act
            await apiClient.PatchFlightCabinCrewMembersRelationshipAsync(flightId, requestDocument);

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Method.Should().Be(HttpMethod.Patch);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/cabin-crew-members");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": [
    {
      ""type"": ""flight-attendants"",
      ""id"": ""bBJHu""
    },
    {
      ""type"": ""flight-attendants"",
      ""id"": ""NInmX""
    }
  ]
}");
        }

        [Fact]
        public async Task Deleting_ToMany_relationship_produces_expected_request()
        {
            // Arrange
            const string flightId = "ZvuH1";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            var requestDocument = new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Id = "bBJHu",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    },
                    new()
                    {
                        Id = "NInmX",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    }
                }
            };

            // Act
            await apiClient.DeleteFlightCabinCrewMembersRelationshipAsync(flightId, requestDocument);

            // Assert
            wrapper.Request.ShouldNotBeNull();
            wrapper.Request.Method.Should().Be(HttpMethod.Delete);
            wrapper.Request.RequestUri.Should().Be(HostPrefix + $"flights/{flightId}/relationships/cabin-crew-members");
            wrapper.Request.Content.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
            wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(HeaderConstants.MediaType);

            wrapper.RequestBody.Should().BeJson(@"{
  ""data"": [
    {
      ""type"": ""flight-attendants"",
      ""id"": ""bBJHu""
    },
    {
      ""type"": ""flight-attendants"",
      ""id"": ""NInmX""
    }
  ]
}");
        }
    }
}
