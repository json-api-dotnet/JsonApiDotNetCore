using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    public sealed class SerializationTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> _testContext;
        private readonly SerializationFakers _fakers = new SerializationFakers();

        public SerializationTests(ExampleIntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeExceptionStackTraceInErrors = false;
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_get_primary_resources_with_include()
        {
            // Arrange
            var meetings = _fakers.Meeting.Generate(1);
            meetings[0].Attendees = _fakers.MeetingAttendee.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Meeting>();
                dbContext.Meetings.AddRange(meetings);
                await dbContext.SaveChangesAsync();
            });

            var route = "/meetings?include=attendees";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings?include=attendees"",
    ""first"": ""http://localhost/meetings?include=attendees""
  },
  ""data"": [
    {
      ""type"": ""meetings"",
      ""id"": """ + meetings[0].StringId + @""",
      ""attributes"": {
        ""title"": """ + meetings[0].Title + @""",
        ""startTime"": """ + meetings[0].StartTime.ToString("O") + @""",
        ""duration"": """ + meetings[0].Duration + @""",
        ""location"": {
          ""lat"": " + meetings[0].Location.Latitude.ToString(CultureInfo.InvariantCulture) + @",
          ""lng"": " + meetings[0].Location.Longitude.ToString(CultureInfo.InvariantCulture) + @"
        }
      },
      ""relationships"": {
        ""attendees"": {
          ""links"": {
            ""self"": ""http://localhost/meetings/" + meetings[0].StringId + @"/relationships/attendees"",
            ""related"": ""http://localhost/meetings/" + meetings[0].StringId + @"/attendees""
          },
          ""data"": [
            {
              ""type"": ""meetingAttendees"",
              ""id"": """ + meetings[0].Attendees[0].StringId + @"""
            }
          ]
        }
      },
      ""links"": {
        ""self"": ""http://localhost/meetings/" + meetings[0].StringId + @"""
      }
    }
  ],
  ""included"": [
    {
      ""type"": ""meetingAttendees"",
      ""id"": """ + meetings[0].Attendees[0].StringId + @""",
      ""attributes"": {
        ""displayName"": """ + meetings[0].Attendees[0].DisplayName + @"""
      },
      ""relationships"": {
        ""meeting"": {
          ""links"": {
            ""self"": ""http://localhost/meetingAttendees/" + meetings[0].Attendees[0].StringId + @"/relationships/meeting"",
            ""related"": ""http://localhost/meetingAttendees/" + meetings[0].Attendees[0].StringId + @"/meeting""
          }
        }
      },
      ""links"": {
        ""self"": ""http://localhost/meetingAttendees/" + meetings[0].Attendees[0].StringId + @"""
      }
    }
  ]
}");
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            var meeting = _fakers.Meeting.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Meetings.Add(meeting);
                await dbContext.SaveChangesAsync();
            });

            var route = "/meetings/" + meeting.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"""
  },
  ""data"": {
    ""type"": ""meetings"",
    ""id"": """ + meeting.StringId + @""",
    ""attributes"": {
      ""title"": """ + meeting.Title + @""",
      ""startTime"": """ + meeting.StartTime.ToString("O") + @""",
      ""duration"": """ + meeting.Duration + @""",
      ""location"": {
        ""lat"": " + meeting.Location.Latitude.ToString(CultureInfo.InvariantCulture) + @",
        ""lng"": " + meeting.Location.Longitude.ToString(CultureInfo.InvariantCulture) + @"
      }
    },
    ""relationships"": {
      ""attendees"": {
        ""links"": {
          ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees"",
          ""related"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees""
        }
      }
    },
    ""links"": {
      ""self"": ""http://localhost/meetings/" + meeting.StringId + @"""
    }
  }
}");
        }

        [Fact]
        public async Task Cannot_get_unknown_primary_resource_by_ID()
        {
            // Arrange
            var unknownId = Guid.NewGuid();

            var route = "/meetings/" + unknownId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            var jObject = JsonConvert.DeserializeObject<JObject>(responseDocument);
            var errorId = jObject["errors"].Should().NotBeNull().And.Subject.Select(element => (string) element["id"]).Single();

            responseDocument.Should().BeJson(@"{
  ""errors"": [
    {
      ""id"": """ + errorId + @""",
      ""status"": ""404"",
      ""title"": ""The requested resource does not exist."",
      ""detail"": ""Resource of type 'meetings' with ID '" + unknownId + @"' does not exist.""
    }
  ]
}");
        }

        [Fact]
        public async Task Can_get_secondary_resource()
        {
            // Arrange
            var attendee = _fakers.MeetingAttendee.Generate();
            attendee.Meeting = _fakers.Meeting.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Attendees.Add(attendee);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetingAttendees/{attendee.StringId}/meeting";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/meeting""
  },
  ""data"": {
    ""type"": ""meetings"",
    ""id"": """ + attendee.Meeting.StringId + @""",
    ""attributes"": {
      ""title"": """ + attendee.Meeting.Title + @""",
      ""startTime"": """ + attendee.Meeting.StartTime.ToString("O") + @""",
      ""duration"": """ + attendee.Meeting.Duration + @""",
      ""location"": {
        ""lat"": " + attendee.Meeting.Location.Latitude.ToString(CultureInfo.InvariantCulture) + @",
        ""lng"": " + attendee.Meeting.Location.Longitude.ToString(CultureInfo.InvariantCulture) + @"
      }
    },
    ""relationships"": {
      ""attendees"": {
        ""links"": {
          ""self"": ""http://localhost/meetings/" + attendee.Meeting.StringId + @"/relationships/attendees"",
          ""related"": ""http://localhost/meetings/" + attendee.Meeting.StringId + @"/attendees""
        }
      }
    },
    ""links"": {
      ""self"": ""http://localhost/meetings/" + attendee.Meeting.StringId + @"""
    }
  }
}");
        }

        [Fact]
        public async Task Can_get_unknown_secondary_resource()
        {
            // Arrange
            var attendee = _fakers.MeetingAttendee.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Attendees.Add(attendee);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetingAttendees/{attendee.StringId}/meeting";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/meeting""
  },
  ""data"": null
}");
        }

        [Fact]
        public async Task Can_get_secondary_resources()
        {
            // Arrange
            var meeting = _fakers.Meeting.Generate();
            meeting.Attendees = _fakers.MeetingAttendee.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Meetings.Add(meeting);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetings/{meeting.StringId}/attendees";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees""
  },
  ""data"": [
    {
      ""type"": ""meetingAttendees"",
      ""id"": """ + meeting.Attendees[0].StringId + @""",
      ""attributes"": {
        ""displayName"": """ + meeting.Attendees[0].DisplayName + @"""
      },
      ""relationships"": {
        ""meeting"": {
          ""links"": {
            ""self"": ""http://localhost/meetingAttendees/" + meeting.Attendees[0].StringId + @"/relationships/meeting"",
            ""related"": ""http://localhost/meetingAttendees/" + meeting.Attendees[0].StringId + @"/meeting""
          }
        }
      },
      ""links"": {
        ""self"": ""http://localhost/meetingAttendees/" + meeting.Attendees[0].StringId + @"""
      }
    }
  ]
}");
        }

        [Fact]
        public async Task Can_get_unknown_secondary_resources()
        {
            // Arrange
            var meeting = _fakers.Meeting.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Meetings.Add(meeting);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetings/{meeting.StringId}/attendees";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees""
  },
  ""data"": []
}");
        }

        [Fact]
        public async Task Can_get_HasOne_relationship()
        {
            // Arrange
            var attendee = _fakers.MeetingAttendee.Generate();
            attendee.Meeting = _fakers.Meeting.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Attendees.Add(attendee);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetingAttendees/{attendee.StringId}/relationships/meeting";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/relationships/meeting"",
    ""related"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/meeting""
  },
  ""data"": {
    ""type"": ""meetings"",
    ""id"": """ + attendee.Meeting.StringId + @"""
  }
}");
        }

        [Fact]
        public async Task Can_get_HasMany_relationship()
        {
            // Arrange
            var meeting = _fakers.Meeting.Generate();
            meeting.Attendees = _fakers.MeetingAttendee.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Meetings.Add(meeting);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/meetings/{meeting.StringId}/relationships/attendees";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            var meetingIds = meeting.Attendees.Select(attendee => attendee.StringId).OrderBy(id => id).ToArray();

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees"",
    ""related"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees""
  },
  ""data"": [
    {
      ""type"": ""meetingAttendees"",
      ""id"": """ + meetingIds[0] + @"""
    },
    {
      ""type"": ""meetingAttendees"",
      ""id"": """ + meetingIds[1] + @"""
    }
  ]
}");
        }

        [Fact]
        public async Task Can_create_resource_with_side_effects()
        {
            // Arrange
            var newMeeting = _fakers.Meeting.Generate();
            newMeeting.Id = Guid.NewGuid();

            var requestBody = new
            {
                data = new
                {
                    type = "meetings",
                    id = newMeeting.StringId,
                    attributes = new
                    {
                        title = newMeeting.Title,
                        startTime = newMeeting.StartTime,
                        duration = newMeeting.Duration,
                        location = new
                        {
                            lat = newMeeting.Latitude,
                            lng = newMeeting.Longitude
                        }
                    }
                }
            };

            var route = "/meetings";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings""
  },
  ""data"": {
    ""type"": ""meetings"",
    ""id"": """ + newMeeting.StringId + @""",
    ""attributes"": {
      ""title"": """ + newMeeting.Title + @""",
      ""startTime"": """ + newMeeting.StartTime.ToString("O") + @""",
      ""duration"": """ + newMeeting.Duration + @""",
      ""location"": {
        ""lat"": " + newMeeting.Location.Latitude.ToString(CultureInfo.InvariantCulture) + @",
        ""lng"": " + newMeeting.Location.Longitude.ToString(CultureInfo.InvariantCulture) + @"
      }
    },
    ""relationships"": {
      ""attendees"": {
        ""links"": {
          ""self"": ""http://localhost/meetings/" + newMeeting.StringId + @"/relationships/attendees"",
          ""related"": ""http://localhost/meetings/" + newMeeting.StringId + @"/attendees""
        }
      }
    },
    ""links"": {
      ""self"": ""http://localhost/meetings/" + newMeeting.StringId + @"""
    }
  }
}");
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects()
        {
            // Arrange
            var existingAttendee = _fakers.MeetingAttendee.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Attendees.Add(existingAttendee);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "meetingAttendees",
                    id = existingAttendee.StringId,
                    attributes = new
                    {
                        displayName = existingAttendee.DisplayName
                    }
                }
            };

            var route = "/meetingAttendees/" + existingAttendee.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + existingAttendee.StringId + @"""
  },
  ""data"": {
    ""type"": ""meetingAttendees"",
    ""id"": """ + existingAttendee.StringId + @""",
    ""attributes"": {
      ""displayName"": """ + existingAttendee.DisplayName + @"""
    },
    ""relationships"": {
      ""meeting"": {
        ""links"": {
          ""self"": ""http://localhost/meetingAttendees/" + existingAttendee.StringId + @"/relationships/meeting"",
          ""related"": ""http://localhost/meetingAttendees/" + existingAttendee.StringId + @"/meeting""
        }
      }
    },
    ""links"": {
      ""self"": ""http://localhost/meetingAttendees/" + existingAttendee.StringId + @"""
    }
  }
}");
        }
    }
}
