using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.Exceptions;
using OpenApiClientTests.LegacyClient.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.LegacyClient
{
    public sealed class ResponseTests
    {
        private const string HostPrefix = "http://localhost/api/v1/";

        [Fact]
        public async Task Getting_resource_collection_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string flightDestination = "Amsterdam";
            const string flightServiceOnBoard = "Movies";
            const string flightDepartsAt = "2014-11-25T00:00:00";
            const string documentMetaValue = "1";
            const string flightMetaValue = "https://api.jsonapi.net/docs/#get-flights";
            const string purserMetaValue = "https://api.jsonapi.net/docs/#get-flight-purser";
            const string cabinCrewMembersMetaValue = "https://api.jsonapi.net/docs/#get-flight-cabin-crew-members";
            const string passengersMetaValue = "https://api.jsonapi.net/docs/#get-flight-passengers";
            const string topLevelLink = HostPrefix + "flights";
            const string flightResourceLink = topLevelLink + "/" + flightId;

            const string responseBody = @"{
  ""meta"": {
    ""total-resources"": """ + documentMetaValue + @"""
  },
  ""links"": {
    ""self"": """ + topLevelLink + @""",
    ""first"": """ + topLevelLink + @""",
    ""last"": """ + topLevelLink + @"""
  },
  ""data"": [
    {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""attributes"": {
        ""final-destination"": """ + flightDestination + @""",
        ""stop-over-destination"": null,
        ""operated-by"": ""DeltaAirLines"",
        ""departs-at"": """ + flightDepartsAt + @""",
        ""arrives-at"": null,
        ""services-on-board"": [
          """ + flightServiceOnBoard + @""",
          """",
          null
        ]
      },
      ""relationships"": {
        ""purser"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/purser"",
            ""related"": """ + flightResourceLink + @"/purser""
          },
          ""meta"": {
             ""docs"": """ + purserMetaValue + @"""
          }
        },
        ""cabin-crew-members"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/cabin-crew-members"",
            ""related"": """ + flightResourceLink + @"/cabin-crew-members""
          },
          ""meta"": {
             ""docs"": """ + cabinCrewMembersMetaValue + @"""
          }
        },
        ""passengers"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/passengers"",
            ""related"": """ + flightResourceLink + @"/passengers""
          },
          ""meta"": {
             ""docs"": """ + passengersMetaValue + @"""
          }
        }
      },
      ""links"": {
        ""self"": """ + flightResourceLink + @"""
      },
      ""meta"": {
        ""docs"": """ + flightMetaValue + @"""
      }
    }
  ]
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightCollectionResponseDocument document = await apiClient.GetFlightCollectionAsync();

            // Assert
            document.Jsonapi.Should().BeNull();
            document.Meta.Should().HaveCount(1);
            document.Meta["total-resources"].Should().Be(documentMetaValue);
            document.Links.Self.Should().Be(topLevelLink);
            document.Links.First.Should().Be(topLevelLink);
            document.Links.Last.Should().Be(topLevelLink);
            document.Data.Should().HaveCount(1);

            FlightDataInResponse flight = document.Data.First();
            flight.Id.Should().Be(flightId);
            flight.Type.Should().Be(FlightsResourceType.Flights);
            flight.Links.Self.Should().Be(flightResourceLink);
            flight.Meta.Should().HaveCount(1);
            flight.Meta["docs"].Should().Be(flightMetaValue);

            flight.Attributes.FinalDestination.Should().Be(flightDestination);
            flight.Attributes.StopOverDestination.Should().BeNull();
            flight.Attributes.ServicesOnBoard.Should().HaveCount(3);
            flight.Attributes.ServicesOnBoard.ElementAt(0).Should().Be(flightServiceOnBoard);
            flight.Attributes.ServicesOnBoard.ElementAt(1).Should().Be(string.Empty);
            flight.Attributes.ServicesOnBoard.ElementAt(2).Should().BeNull();
            flight.Attributes.OperatedBy.Should().Be(Airline.DeltaAirLines);
            flight.Attributes.DepartsAt.Should().Be(DateTimeOffset.Parse(flightDepartsAt, new CultureInfo("en-GB")));
            flight.Attributes.ArrivesAt.Should().BeNull();

            flight.Relationships.Purser.Data.Should().BeNull();
            flight.Relationships.Purser.Links.Self.Should().Be(flightResourceLink + "/relationships/purser");
            flight.Relationships.Purser.Links.Related.Should().Be(flightResourceLink + "/purser");
            flight.Relationships.Purser.Meta.Should().HaveCount(1);
            flight.Relationships.Purser.Meta["docs"].Should().Be(purserMetaValue);

            flight.Relationships.CabinCrewMembers.Data.Should().BeNull();
            flight.Relationships.CabinCrewMembers.Links.Self.Should().Be(flightResourceLink + "/relationships/cabin-crew-members");
            flight.Relationships.CabinCrewMembers.Links.Related.Should().Be(flightResourceLink + "/cabin-crew-members");
            flight.Relationships.CabinCrewMembers.Meta.Should().HaveCount(1);
            flight.Relationships.CabinCrewMembers.Meta["docs"].Should().Be(cabinCrewMembersMetaValue);

            flight.Relationships.Passengers.Data.Should().BeNull();
            flight.Relationships.Passengers.Links.Self.Should().Be(flightResourceLink + "/relationships/passengers");
            flight.Relationships.Passengers.Links.Related.Should().Be(flightResourceLink + "/passengers");
            flight.Relationships.Passengers.Meta.Should().HaveCount(1);
            flight.Relationships.Passengers.Meta["docs"].Should().Be(passengersMetaValue);
        }

        [Fact]
        public async Task Getting_resource_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string departsAtInZuluTime = "2021-06-08T12:53:30.554Z";
            const string arrivesAtWithUtcOffset = "2019-02-20T11:56:33.0721266+01:00";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]=departs-at,arrives-at""
  },
  ""data"": {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""attributes"": {
        ""departs-at"": """ + departsAtInZuluTime + @""",
        ""arrives-at"": """ + arrivesAtWithUtcOffset + @"""
      },
      ""links"": {
        ""self"": """ + HostPrefix + @"flights/" + flightId + @"""
      }
    }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await apiClient.GetFlightAsync(flightId);

            // Assert
            document.Jsonapi.Should().BeNull();
            document.Meta.Should().BeNull();
            document.Data.Meta.Should().BeNull();
            document.Data.Relationships.Should().BeNull();
            document.Data.Attributes.DepartsAt.Should().Be(DateTimeOffset.Parse(departsAtInZuluTime));
            document.Data.Attributes.ArrivesAt.Should().Be(DateTimeOffset.Parse(arrivesAtWithUtcOffset));
            document.Data.Attributes.ServicesOnBoard.Should().BeNull();
            document.Data.Attributes.FinalDestination.Should().BeNull();
            document.Data.Attributes.StopOverDestination.Should().BeNull();
            document.Data.Attributes.OperatedBy.Should().Be(default);
        }

        [Fact]
        public async Task Getting_unknown_resource_translates_error_response()
        {
            // Arrange
            const string flightId = "ZvuH1";

            const string responseBody = @"{
  ""errors"": [
    {
      ""id"": ""f1a520ac-02a0-466b-94ea-86cbaa86f02f"",
      ""status"": ""404"",
      ""destination"": ""The requested resource does not exist."",
      ""detail"": ""Resource of type 'flights' with ID '" + flightId + @"' does not exist.""
    }
  ]
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NotFound, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task<FlightPrimaryResponseDocument>> action = async () => await apiClient.GetFlightAsync(flightId);

            // Assert
            ExceptionAssertions<ApiException> assertion = await action.Should().ThrowExactlyAsync<ApiException>();
            ApiException exception = assertion.Subject.Single();

            exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            exception.Response.Should().Be(responseBody);
        }

        [Fact]
        public async Task Posting_resource_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string flightAttendantId = "bBJHu";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=purser,cabin-crew-members,passengers""
  },
  ""data"": {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""relationships"": {
        ""purser"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/purser"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/purser""
          },
          ""data"": {
              ""type"": ""flight-attendants"",
              ""id"": """ + flightAttendantId + @"""
            }
        },
        ""cabin-crew-members"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-crew-members"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-crew-members""
          },
          ""data"": [
            {
              ""type"": ""flight-attendants"",
              ""id"": """ + flightAttendantId + @"""
            }
          ],
        },
        ""passengers"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/passengers"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/passengers""
          },
          ""data"": [ ]
        }
      },
      ""links"": {
        ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=purser,cabin-crew-members,passengers""
      }
    }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.Created, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await apiClient.PostFlightAsync(new FlightPostRequestDocument
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
                                Id = flightAttendantId,
                                Type = FlightAttendantsResourceType.FlightAttendants
                            }
                        }
                    }
                }
            });

            // Assert
            document.Data.Attributes.Should().BeNull();
            document.Data.Relationships.Purser.Data.Should().NotBeNull();
            document.Data.Relationships.Purser.Data.Id.Should().Be(flightAttendantId);
            document.Data.Relationships.CabinCrewMembers.Data.Should().HaveCount(1);
            document.Data.Relationships.CabinCrewMembers.Data.First().Id.Should().Be(flightAttendantId);
            document.Data.Relationships.CabinCrewMembers.Data.First().Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
            document.Data.Relationships.Passengers.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Patching_resource_with_side_effects_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]""
  },
  ""data"": {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""links"": {
        ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=purser,cabin-crew-members,passengers""
      }
    }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await apiClient.PatchFlightAsync(flightId, new FlightPatchRequestDocument
            {
                Data = new FlightDataInPatchRequest
                {
                    Id = flightId,
                    Type = FlightsResourceType.Flights
                }
            });

            // Assert
            document.Data.Type.Should().Be(FlightsResourceType.Flights);
            document.Data.Attributes.Should().BeNull();
            document.Data.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Patching_resource_without_side_effects_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(async () => await apiClient.PatchFlightAsync(flightId,
                new FlightPatchRequestDocument
                {
                    Data = new FlightDataInPatchRequest
                    {
                        Id = flightId,
                        Type = FlightsResourceType.Flights
                    }
                }));

            // Assert
            document.Should().BeNull();
        }

        [Fact]
        public async Task Deleting_resource_produces_empty_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await apiClient.DeleteFlightAsync("ZvuH1");

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Getting_secondary_resource_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string purserId = "bBJHu";
            const string emailAddress = "email@example.com";
            const string age = "20";
            const string profileImageUrl = "www.image.com";
            const string distanceTraveledInKilometer = "5000";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/purser"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/purser"",
    ""last"": """ + HostPrefix + @"flights/" + flightId + @"/purser""
  },
  ""data"": {
    ""type"": ""flight-attendants"",
    ""id"": """ + purserId + @""",
    ""attributes"": {
      ""email-address"": """ + emailAddress + @""",
      ""age"": """ + age + @""",
      ""profile-image-url"": """ + profileImageUrl + @""",
      ""distance-traveled-in-kilometers"": """ + distanceTraveledInKilometer + @""",
    },
    ""relationships"": {
      ""scheduled-for-flights"": {
        ""links"": {
          ""self"": """ + HostPrefix + @"flight-attendants/" + purserId + @"/relationships/scheduled-for-flights"",
          ""related"": """ + HostPrefix + @"flight-attendants/" + purserId + @"/scheduled-for-flights""
        }
      },
      ""purser-on-flights"": {
        ""links"": {
          ""self"": """ + HostPrefix + @"flight-attendants/" + purserId + @"/relationships/purser-on-flights"",
          ""related"": """ + HostPrefix + @"flight-attendants/" + purserId + @"/purser-on-flights""
        }
      },
    },
    ""links"": {
      ""self"": """ + HostPrefix + @"flight-attendants/" + purserId + @""",
    }
  }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantSecondaryResponseDocument document = await apiClient.GetFlightPurserAsync(flightId);

            // Assert
            document.Data.Should().NotBeNull();
            document.Data.Id.Should().Be(purserId);
            document.Data.Attributes.EmailAddress.Should().Be(emailAddress);
            document.Data.Attributes.Age.Should().Be(int.Parse(age));
            document.Data.Attributes.ProfileImageUrl.Should().Be(profileImageUrl);
            document.Data.Attributes.DistanceTraveledInKilometers.Should().Be(int.Parse(distanceTraveledInKilometer));
        }

        [Fact]
        public async Task Getting_nullable_secondary_resource_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/backup-purser"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/backup-purser"",
    ""last"": """ + HostPrefix + @"flights/" + flightId + @"/backup-purser""
  },
  ""data"": null
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            NullableFlightAttendantSecondaryResponseDocument document = await apiClient.GetFlightBackupPurserAsync(flightId);

            // Assert
            document.Data.Should().BeNull();
        }

        [Fact]
        public async Task Getting_secondary_resources_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-crew-members"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-crew-members""
  },
  ""data"": [ ]
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantCollectionResponseDocument document = await apiClient.GetFlightCabinCrewMembersAsync(flightId);

            // Assert
            document.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Getting_nullable_ToOne_relationship_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/backup-purser"",
    ""related"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/backup-purser""
  },
  ""data"": null
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            NullableFlightAttendantIdentifierResponseDocument document = await apiClient.GetFlightBackupPurserRelationshipAsync(flightId);

            // Assert
            document.Data.Should().BeNull();
        }

        [Fact]
        public async Task Getting_ToOne_relationship_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string purserId = "bBJHu";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/purser"",
    ""related"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/purser""
  },
  ""data"": {
    ""type"": ""flight-attendants"",
    ""id"": """ + purserId + @"""
  }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantIdentifierResponseDocument document = await apiClient.GetFlightPurserRelationshipAsync(flightId);

            // Assert
            document.Data.Should().NotBeNull();
            document.Data.Id.Should().Be(purserId);
            document.Data.Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
        }

        [Fact]
        public async Task Patching_ToOne_relationship_translates_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            await apiClient.PatchFlightPurserRelationshipAsync("ZvuH1", new ToOneFlightAttendantInRequest
            {
                Data = new FlightAttendantIdentifier
                {
                    Id = "Adk2a",
                    Type = FlightAttendantsResourceType.FlightAttendants
                }
            });
        }

        [Fact]
        public async Task Getting_ToMany_relationship_translates_response()
        {
            // Arrange
            const string flightId = "ZvuH1";
            const string flightAttendantId1 = "bBJHu";
            const string flightAttendantId2 = "ZvuHNInmX1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-crew-members"",
    ""related"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-crew-members"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-crew-members""
  },
  ""data"": [{
    ""type"": ""flight-attendants"",
    ""id"": """ + flightAttendantId1 + @"""
  },
  {
    ""type"": ""flight-attendants"",
    ""id"": """ + flightAttendantId2 + @"""
  }]
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantIdentifierCollectionResponseDocument document = await apiClient.GetFlightCabinCrewMembersRelationshipAsync(flightId);

            // Assert
            document.Data.Should().HaveCount(2);
            document.Data.First().Id.Should().Be(flightAttendantId1);
            document.Data.First().Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
            document.Data.Last().Id.Should().Be(flightAttendantId2);
            document.Data.Last().Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
        }

        [Fact]
        public async Task Posting_ToMany_relationship_produces_empty_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await apiClient.PostFlightCabinCrewMembersRelationshipAsync("ZvuH1", new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Id = "Adk2a",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    },
                    new()
                    {
                        Id = "Un37k",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    }
                }
            });

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Patching_ToMany_relationship_produces_empty_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await apiClient.PatchFlightCabinCrewMembersRelationshipAsync("ZvuH1", new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Id = "Adk2a",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    },
                    new()
                    {
                        Id = "Un37k",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    }
                }
            });

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Deleting_ToMany_relationship_produces_empty_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient apiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await apiClient.DeleteFlightCabinCrewMembersRelationshipAsync("ZvuH1", new ToManyFlightAttendantInRequest
            {
                Data = new List<FlightAttendantIdentifier>
                {
                    new()
                    {
                        Id = "Adk2a",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    },
                    new()
                    {
                        Id = "Un37k",
                        Type = FlightAttendantsResourceType.FlightAttendants
                    }
                }
            });

            // Assert
            await action.Should().NotThrowAsync();
        }
    }
}
