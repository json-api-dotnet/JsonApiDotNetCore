using System.Net;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.Net.Http.Headers;
using OpenApiNSwagClientTests.LegacyOpenApi.GeneratedCode;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagClientTests.LegacyOpenApi;

public sealed class RequestTests
{
    private const string OpenApiMediaType = "application/vnd.api+json; ext=openapi";
    private const string HostPrefix = "http://localhost/api/";

    [Fact]
    public async Task Getting_resource_collection_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCollectionAsync(null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Getting_resource_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightAsync(flightId, null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Partial_posting_resource_with_selected_relationships_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new CreateFlightRequestDocument
        {
            Data = new DataInCreateFlightRequest
            {
                Relationships = new RelationshipsInCreateFlightRequest
                {
                    Purser = new ToOneFlightAttendantInRequest
                    {
                        Data = new FlightAttendantIdentifierInRequest
                        {
                            Id = "bBJHu"
                        }
                    },
                    BackupPurser = new NullableToOneFlightAttendantInRequest
                    {
                        Data = new FlightAttendantIdentifierInRequest
                        {
                            Id = "NInmX"
                        }
                    }
                }
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostFlightAsync(null, requestBody));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": {
                "type": "flights",
                "relationships": {
                  "openapi:discriminator": "flights",
                  "purser": {
                    "data": {
                      "type": "flight-attendants",
                      "id": "bBJHu"
                    }
                  },
                  "backup-purser": {
                    "data": {
                      "type": "flight-attendants",
                      "id": "NInmX"
                    }
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Partial_posting_resource_produces_expected_request()
    {
        // Arrange
        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        const char euroSign = '\x20AC';
        const char checkMark = '\x2713';
        const char capitalLWithStroke = '\x0141';

        string specialCharacters = new(new[]
        {
            euroSign,
            checkMark,
            capitalLWithStroke
        });

        string name = $"anAirplaneName {specialCharacters}";

        var requestBody = new CreateAirplaneRequestDocument
        {
            Data = new DataInCreateAirplaneRequest
            {
                Attributes = new TrackChangesFor<AttributesInCreateAirplaneRequest>(apiClient)
                {
                    Initializer =
                    {
                        Name = name,
                        AirtimeInHours = 800,
                        SerialNumber = null
                    }
                }.Initializer
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PostAirplaneAsync(null, requestBody));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}airplanes");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson($$"""
            {
              "data": {
                "type": "airplanes",
                "attributes": {
                  "openapi:discriminator": "airplanes",
                  "name": "{{name}}",
                  "serial-number": null,
                  "airtime-in-hours": 800
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Partial_patching_resource_produces_expected_request()
    {
        // Arrange
        const string airplaneId = "XUuiP";
        var lastServicedAt = 1.January(2021).At(15, 23, 5, 33).ToDateTimeOffset(4.Hours());

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new UpdateAirplaneRequestDocument
        {
            Data = new DataInUpdateAirplaneRequest
            {
                Id = airplaneId,
                Attributes = new TrackChangesFor<AttributesInUpdateAirplaneRequest>(apiClient)
                {
                    Initializer =
                    {
                        LastServicedAt = lastServicedAt,
                        SerialNumber = null,
                        IsInMaintenance = false,
                        AirtimeInHours = null
                    }
                }.Initializer
            }
        };

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.PatchAirplaneAsync(airplaneId, null, requestBody));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}airplanes/{airplaneId}");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": {
                "type": "airplanes",
                "id": "XUuiP",
                "attributes": {
                  "openapi:discriminator": "airplanes",
                  "serial-number": null,
                  "airtime-in-hours": null,
                  "last-serviced-at": "2021-01-01T15:23:05.033+04:00",
                  "is-in-maintenance": false
                }
              }
            }
            """);
    }

    [Fact]
    public async Task Deleting_resource_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        await apiClient.DeleteFlightAsync(flightId);

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Method.Should().Be(HttpMethod.Delete);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Getting_secondary_resource_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightPurserAsync(flightId, null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/purser");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Getting_secondary_resources_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCabinCrewMembersAsync(flightId, null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/cabin-crew-members");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Getting_ToOne_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightPurserRelationshipAsync(flightId, null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/purser");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Patching_ToOne_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new ToOneFlightAttendantInRequest
        {
            Data = new FlightAttendantIdentifierInRequest
            {
                Id = "bBJHu"
            }
        };

        // Act
        await apiClient.PatchFlightPurserRelationshipAsync(flightId, requestBody);

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/purser");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": {
                "type": "flight-attendants",
                "id": "bBJHu"
              }
            }
            """);
    }

    [Fact]
    public async Task Getting_ToMany_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        // Act
        _ = await ApiResponse.TranslateAsync(async () => await apiClient.GetFlightCabinCrewMembersRelationshipAsync(flightId, null, null));

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Headers.GetValue(HeaderNames.Accept).Should().Be(OpenApiMediaType);
        wrapper.Request.Method.Should().Be(HttpMethod.Get);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/cabin-crew-members");
        wrapper.RequestBody.Should().BeNull();
    }

    [Fact]
    public async Task Posting_ToMany_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new ToManyFlightAttendantInRequest
        {
            Data =
            [
                new FlightAttendantIdentifierInRequest
                {
                    Id = "bBJHu"
                },
                new FlightAttendantIdentifierInRequest
                {
                    Id = "NInmX"
                }
            ]
        };

        // Act
        await apiClient.PostFlightCabinCrewMembersRelationshipAsync(flightId, requestBody);

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Method.Should().Be(HttpMethod.Post);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/cabin-crew-members");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": [
                {
                  "type": "flight-attendants",
                  "id": "bBJHu"
                },
                {
                  "type": "flight-attendants",
                  "id": "NInmX"
                }
              ]
            }
            """);
    }

    [Fact]
    public async Task Patching_ToMany_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new ToManyFlightAttendantInRequest
        {
            Data =
            [
                new FlightAttendantIdentifierInRequest
                {
                    Id = "bBJHu"
                },
                new FlightAttendantIdentifierInRequest
                {
                    Id = "NInmX"
                }
            ]
        };

        // Act
        await apiClient.PatchFlightCabinCrewMembersRelationshipAsync(flightId, requestBody);

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Method.Should().Be(HttpMethod.Patch);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/cabin-crew-members");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": [
                {
                  "type": "flight-attendants",
                  "id": "bBJHu"
                },
                {
                  "type": "flight-attendants",
                  "id": "NInmX"
                }
              ]
            }
            """);
    }

    [Fact]
    public async Task Deleting_ToMany_relationship_produces_expected_request()
    {
        // Arrange
        const string flightId = "ZvuH1";

        using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
        var apiClient = new LegacyClient(wrapper.HttpClient);

        var requestBody = new ToManyFlightAttendantInRequest
        {
            Data =
            [
                new FlightAttendantIdentifierInRequest
                {
                    Id = "bBJHu"
                },
                new FlightAttendantIdentifierInRequest
                {
                    Id = "NInmX"
                }
            ]
        };

        // Act
        await apiClient.DeleteFlightCabinCrewMembersRelationshipAsync(flightId, requestBody);

        // Assert
        wrapper.Request.Should().NotBeNull();
        wrapper.Request.Method.Should().Be(HttpMethod.Delete);
        wrapper.Request.RequestUri.Should().Be($"{HostPrefix}flights/{flightId}/relationships/cabin-crew-members");
        wrapper.Request.Content.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType.Should().NotBeNull();
        wrapper.Request.Content!.Headers.ContentType!.ToString().Should().Be(OpenApiMediaType);

        wrapper.RequestBody.Should().BeJson("""
            {
              "data": [
                {
                  "type": "flight-attendants",
                  "id": "bBJHu"
                },
                {
                  "type": "flight-attendants",
                  "id": "NInmX"
                }
              ]
            }
            """);
    }
}
