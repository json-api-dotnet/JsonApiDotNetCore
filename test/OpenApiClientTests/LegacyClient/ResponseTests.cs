using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;
using OpenApiClientTests.LegacyClient.GeneratedCode;
using Xunit;

#pragma warning disable AV1704 // Don't include numbers in variables, parameters and type members

namespace OpenApiClientTests.LegacyClient
{
    public sealed class ResponseTests
    {
        private const string HostPrefix = "http://localhost/api/v1/";

        [Fact]
        public async Task Getting_resource_collection_translates_response()
        {
            // Arrange
            const string flightId = "8712";
            const string flightDestination = "Destination of Flight";
            const string fightServiceOnBoard = "Movies";
            const string flightDepartsAt = "2014-11-25T00:00:00";
            const string documentMetaValue = "1";
            const string flightMetaValue = "https://api.jsonapi.net/docs/#get-flights";
            const string operatingAirplaneMetaValue = "https://jsonapi.net/api/docs/#get-flight-operating-airplane";
            const string cabinPersonnelMetaValue = "https://jsonapi.net/api/docs/#get-flight-cabin-personnel";
            const string reserveCabinPersonnelMetaValue = "https://jsonapi.net/api/docs/#get-flight-backup-cabin-personnel";
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
          """ + fightServiceOnBoard + @""",
          """",
          null
        ]
      },
      ""relationships"": {
        ""operating-airplane"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/operating-airplane"",
            ""related"": """ + flightResourceLink + @"/operating-airplane""
          },
          ""meta"": {
             ""docs"": """ + operatingAirplaneMetaValue + @"""
          }
        },
        ""cabin-personnel"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/cabin-personnel"",
            ""related"": """ + flightResourceLink + @"/cabin-personnel""
          },
          ""meta"": {
             ""docs"": """ + cabinPersonnelMetaValue + @"""
          }
        },
        ""backup-cabin-personnel"": {
          ""links"": {
            ""self"": """ + flightResourceLink + @"/relationships/backup-cabin-personnel"",
            ""related"": """ + flightResourceLink + @"/backup-cabin-personnel""
          },
          ""meta"": {
             ""docs"": """ + reserveCabinPersonnelMetaValue + @"""
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightCollectionResponseDocument document = await openApiClient.GetFlightCollectionAsync();

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
            flight.Attributes.ServicesOnBoard.Should().HaveCount(3);
            flight.Attributes.ServicesOnBoard.ElementAt(0).Should().Be(fightServiceOnBoard);
            flight.Attributes.ServicesOnBoard.ElementAt(1).Should().Be(string.Empty);
            flight.Attributes.ServicesOnBoard.ElementAt(2).Should().BeNull();
            flight.Attributes.OperatedBy.Should().Be(Airline.DeltaAirLines);
            flight.Attributes.DepartsAt.Should().Be(DateTimeOffset.Parse(flightDepartsAt, new CultureInfo("en-GB")));
            flight.Attributes.ArrivesAt.Should().Be(null);

            flight.Relationships.OperatingAirplane.Data.Should().BeNull();
            flight.Relationships.OperatingAirplane.Links.Self.Should().Be(flightResourceLink + "/relationships/operating-airplane");
            flight.Relationships.OperatingAirplane.Links.Related.Should().Be(flightResourceLink + "/operating-airplane");
            flight.Relationships.OperatingAirplane.Meta.Should().HaveCount(1);
            flight.Relationships.OperatingAirplane.Meta["docs"].Should().Be(operatingAirplaneMetaValue);

            flight.Relationships.CabinPersonnel.Data.Should().BeNull();
            flight.Relationships.CabinPersonnel.Links.Self.Should().Be(flightResourceLink + "/relationships/cabin-personnel");
            flight.Relationships.CabinPersonnel.Links.Related.Should().Be(flightResourceLink + "/cabin-personnel");
            flight.Relationships.CabinPersonnel.Meta.Should().HaveCount(1);
            flight.Relationships.CabinPersonnel.Meta["docs"].Should().Be(cabinPersonnelMetaValue);

            flight.Relationships.BackupCabinPersonnel.Data.Should().BeNull();
            flight.Relationships.BackupCabinPersonnel.Links.Self.Should().Be(flightResourceLink + "/relationships/backup-cabin-personnel");
            flight.Relationships.BackupCabinPersonnel.Links.Related.Should().Be(flightResourceLink + "/backup-cabin-personnel");
            flight.Relationships.BackupCabinPersonnel.Meta.Should().HaveCount(1);
            flight.Relationships.BackupCabinPersonnel.Meta["docs"].Should().Be(reserveCabinPersonnelMetaValue);
        }

        [Fact]
        public async Task Getting_resource_translates_response()
        {
            // Arrange
            const string flightId = "8712";
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await openApiClient.GetFlightAsync(Convert.ToInt32(flightId));

            // Assert
            document.Jsonapi.Should().BeNull();
            document.Meta.Should().BeNull();
            document.Data.Meta.Should().BeNull();
            document.Data.Relationships.Should().BeNull();
            document.Data.Attributes.DepartsAt.Should().Be(DateTimeOffset.Parse(departsAtInZuluTime));
            document.Data.Attributes.ArrivesAt.Should().Be(DateTimeOffset.Parse(arrivesAtWithUtcOffset));
            document.Data.Attributes.ServicesOnBoard.Should().BeNull();
            document.Data.Attributes.FinalDestination.Should().BeNull();
            document.Data.Attributes.OperatedBy.Should().Be(default(Airline));
        }

        [Fact]
        public async Task Getting_unknown_resource_translates_error_response()
        {
            // Arrange
            const string flightId = "8712";

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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task<FlightPrimaryResponseDocument>> action = async () => await openApiClient.GetFlightAsync(Convert.ToInt32(flightId));

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
            const string flightId = "8712";
            const string flightAttendantId = "bBJHu";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=operating-airplane,cabin-personnel,backup-cabin-personnel""
  },
  ""data"": {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""relationships"": {
        ""operating-airplane"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/operating-airplane"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/operating-airplane""
          },
          ""data"": null
        },
        ""cabin-personnel"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-personnel"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-personnel""
          },
          ""data"": [
            {
              ""type"": ""flight-attendants"",
              ""id"": """ + flightAttendantId + @"""
            }
          ],
        },
        ""backup-cabin-personnel"": {
          ""links"": {
            ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/backup-cabin-personnel"",
            ""related"": """ + HostPrefix + @"flights/" + flightId + @"/backup-cabin-personnel""
          },
          ""data"": [ ]
        }
      },
      ""links"": {
        ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=operating-airplane,cabin-personnel,backup-cabin-personnel""
      }
    }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.Created, responseBody);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await openApiClient.PostFlightAsync(new FlightPostRequestDocument
            {
                Data = new FlightDataInPostRequest
                {
                    Type = FlightsResourceType.Flights,
                    Relationships = new FlightRelationshipsInPostRequest
                    {
                        OperatingAirplane = new ToOneAirplaneRequestData
                        {
                            Data = new AirplaneIdentifier
                            {
                                Id = "XxuIu",
                                Type = AirplanesResourceType.Airplanes
                            }
                        }
                    }
                }
            });

            // Assert
            document.Data.Attributes.Should().BeNull();
            document.Data.Relationships.OperatingAirplane.Data.Should().BeNull();
            document.Data.Relationships.CabinPersonnel.Data.Should().HaveCount(1);
            document.Data.Relationships.CabinPersonnel.Data.First().Id.Should().Be(flightAttendantId);
            document.Data.Relationships.CabinPersonnel.Data.First().Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
            document.Data.Relationships.BackupCabinPersonnel.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Patching_resource_with_side_effects_translates_response()
        {
            // Arrange
            const string flightId = "8712";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]""
  },
  ""data"": {
      ""type"": ""flights"",
      ""id"": """ + flightId + @""",
      ""links"": {
        ""self"": """ + HostPrefix + @"flights/" + flightId + @"&fields[flights]&include=operating-airplane,cabin-personnel,backup-cabin-personnel""
      }
    }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await openApiClient.PatchFlightAsync(Convert.ToInt32(flightId), new FlightPatchRequestDocument
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
            const string flightId = "8712";
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightPrimaryResponseDocument document = await ApiResponse.TranslateAsync(async () => await openApiClient.PatchFlightAsync(
                Convert.ToInt32(flightId), new FlightPatchRequestDocument
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await openApiClient.DeleteFlightAsync(8712);

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Getting_secondary_resource_translates_response()
        {
            // Arrange
            const string flightId = "8712";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/operating-airplane"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/operating-airplane"",
    ""last"": """ + HostPrefix + @"flights/" + flightId + @"/operating-airplane""
  },
  ""data"": null
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            AirplaneSecondaryResponseDocument document = await openApiClient.GetFlightOperatingAirplaneAsync(Convert.ToInt32(flightId));

            // Assert
            document.Data.Should().BeNull();
        }

        [Fact]
        public async Task Getting_secondary_resources_translates_response()
        {
            // Arrange
            const string flightId = "8712";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-personnel"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/cabin-personnel""
  },
  ""data"": [ ]
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantCollectionResponseDocument document = await openApiClient.GetFlightCabinPersonnelAsync(Convert.ToInt32(flightId));

            // Assert
            document.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Getting_ToOne_relationship_translates_response()
        {
            // Arrange
            const string flightId = "8712";
            const string operatingAirplaneId = "bBJHu";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/operating-airplane"",
    ""related"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/operating-airplane""
  },
  ""data"": {
    ""type"": ""airplanes"",
    ""id"": """ + operatingAirplaneId + @"""
  }
}";

            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.OK, responseBody);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            AirplaneIdentifierResponseDocument document = await openApiClient.GetFlightOperatingAirplaneRelationshipAsync(Convert.ToInt32(flightId));

            // Assert
            document.Data.Should().NotBeNull();
            document.Data.Id.Should().Be(operatingAirplaneId);
            document.Data.Type.Should().Be(FlightAttendantsResourceType.FlightAttendants);
        }

        [Fact]
        public async Task Patching_ToOne_relationship_translates_response()
        {
            // Arrange
            using var wrapper = FakeHttpClientWrapper.Create(HttpStatusCode.NoContent, null);
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            await openApiClient.PatchFlightOperatingAirplaneRelationshipAsync(8712, new ToOneAirplaneRequestData
            {
                Data = new AirplaneIdentifier
                {
                    Id = "Adk2a",
                    Type = AirplanesResourceType.Airplanes
                }
            });
        }

        [Fact]
        public async Task Getting_ToMany_relationship_translates_response()
        {
            // Arrange
            const string flightId = "8712";
            const string flightAttendantId1 = "bBJHu";
            const string flightAttendantId2 = "ZvuHNInmX1";

            const string responseBody = @"{
  ""links"": {
    ""self"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-personnel"",
    ""related"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-personnel"",
    ""first"": """ + HostPrefix + @"flights/" + flightId + @"/relationships/cabin-personnel""
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            FlightAttendantIdentifierCollectionResponseDocument document =
                await openApiClient.GetFlightCabinPersonnelRelationshipAsync(Convert.ToInt32(flightId));

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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await openApiClient.PostFlightCabinPersonnelRelationshipAsync(8712, new ToManyFlightAttendantRequestData
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await openApiClient.PatchFlightCabinPersonnelRelationshipAsync(8712, new ToManyFlightAttendantRequestData
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
            IOpenApiClient openApiClient = new OpenApiClient(wrapper.HttpClient);

            // Act
            Func<Task> action = async () => await openApiClient.DeleteFlightCabinPersonnelRelationshipAsync(8712, new ToManyFlightAttendantRequestData
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
