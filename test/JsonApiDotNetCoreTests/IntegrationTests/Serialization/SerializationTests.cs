using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

public sealed class SerializationTests : IClassFixture<IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext>>
{
    private const string JsonDateTimeOffsetFormatSpecifier = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

    private readonly IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> _testContext;
    private readonly SerializationFakers _fakers = new();

    public SerializationTests(IntegrationTestContext<TestableStartup<SerializationDbContext>, SerializationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<MeetingAttendeesController>();
        testContext.UseController<MeetingsController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
        });

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeExceptionStackTraceInErrors = false;
        options.AllowClientGeneratedIds = true;
        options.IncludeJsonApiVersion = false;
        options.IncludeTotalResourceCount = true;

        if (!options.SerializerOptions.Converters.Any(converter => converter is JsonTimeSpanConverter))
        {
            options.SerializerOptions.Converters.Add(new JsonTimeSpanConverter());
        }
    }

    [Fact]
    public async Task Returns_no_body_for_successful_HEAD_request()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetings/{meeting.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteHeadAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentLength.Should().BeGreaterThan(0);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_no_body_for_failed_HEAD_request()
    {
        // Arrange
        string route = $"/meetings/{Unknown.StringId.For<Meeting, Guid>()}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteHeadAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        httpResponse.Content.Headers.ContentLength.Should().BeGreaterThan(0);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_get_primary_resources_with_include()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();
        meeting.Attendees = _fakers.MeetingAttendee.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings?include=attendees";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings?include=attendees"",
    ""first"": ""http://localhost/meetings?include=attendees"",
    ""last"": ""http://localhost/meetings?include=attendees""
  },
  ""data"": [
    {
      ""type"": ""meetings"",
      ""id"": """ + meeting.StringId + @""",
      ""attributes"": {
        ""title"": """ + meeting.Title + @""",
        ""startTime"": """ + meeting.StartTime.ToString(JsonDateTimeOffsetFormatSpecifier) + @""",
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
          },
          ""data"": [
            {
              ""type"": ""meetingAttendees"",
              ""id"": """ + meeting.Attendees[0].StringId + @"""
            }
          ]
        }
      },
      ""links"": {
        ""self"": ""http://localhost/meetings/" + meeting.StringId + @"""
      }
    }
  ],
  ""included"": [
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
  ],
  ""meta"": {
    ""total"": 1
  }
}");
    }

    [Fact]
    public async Task Can_get_primary_resource_with_empty_ToOne_include()
    {
        // Arrange
        MeetingAttendee attendee = _fakers.MeetingAttendee.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(attendee);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetingAttendees/{attendee.StringId}?include=meeting";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"?include=meeting""
  },
  ""data"": {
    ""type"": ""meetingAttendees"",
    ""id"": """ + attendee.StringId + @""",
    ""attributes"": {
      ""displayName"": """ + attendee.DisplayName + @"""
    },
    ""relationships"": {
      ""meeting"": {
        ""links"": {
          ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/relationships/meeting"",
          ""related"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/meeting""
        },
        ""data"": null
      }
    },
    ""links"": {
      ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"""
    }
  },
  ""included"": []
}");
    }

    [Fact]
    public async Task Can_get_primary_resources_with_empty_ToMany_include()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Meeting>();
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/meetings/?include=attendees";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/?include=attendees"",
    ""first"": ""http://localhost/meetings/?include=attendees"",
    ""last"": ""http://localhost/meetings/?include=attendees""
  },
  ""data"": [
    {
      ""type"": ""meetings"",
      ""id"": """ + meeting.StringId + @""",
      ""attributes"": {
        ""title"": """ + meeting.Title + @""",
        ""startTime"": """ + meeting.StartTime.ToString(JsonDateTimeOffsetFormatSpecifier) + @""",
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
          },
          ""data"": []
        }
      },
      ""links"": {
        ""self"": ""http://localhost/meetings/" + meeting.StringId + @"""
      }
    }
  ],
  ""included"": [],
  ""meta"": {
    ""total"": 1
  }
}");
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetings/{meeting.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

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
      ""startTime"": """ + meeting.StartTime.ToString(JsonDateTimeOffsetFormatSpecifier) + @""",
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
        string meetingId = Unknown.StringId.For<Meeting, Guid>();

        string route = $"/meetings/{meetingId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        string errorId = JsonApiStringConverter.ExtractErrorId(responseDocument);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/ffffffff-ffff-ffff-ffff-ffffffffffff""
  },
  ""errors"": [
    {
      ""id"": """ + errorId + @""",
      ""status"": ""404"",
      ""title"": ""The requested resource does not exist."",
      ""detail"": ""Resource of type 'meetings' with ID '" + meetingId + @"' does not exist.""
    }
  ]
}");
    }

    [Fact]
    public async Task Can_get_secondary_resource()
    {
        // Arrange
        MeetingAttendee attendee = _fakers.MeetingAttendee.Generate();
        attendee.Meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(attendee);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetingAttendees/{attendee.StringId}/meeting";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

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
      ""startTime"": """ + attendee.Meeting.StartTime.ToString(JsonDateTimeOffsetFormatSpecifier) + @""",
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
        MeetingAttendee attendee = _fakers.MeetingAttendee.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(attendee);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetingAttendees/{attendee.StringId}/meeting";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

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
        Meeting meeting = _fakers.Meeting.Generate();
        meeting.Attendees = _fakers.MeetingAttendee.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetings/{meeting.StringId}/attendees";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""last"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees""
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
  ],
  ""meta"": {
    ""total"": 1
  }
}");
    }

    [Fact]
    public async Task Can_get_unknown_secondary_resources()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetings/{meeting.StringId}/attendees";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees""
  },
  ""data"": [],
  ""meta"": {
    ""total"": 0
  }
}");
    }

    [Fact]
    public async Task Can_get_ToOne_relationship()
    {
        // Arrange
        MeetingAttendee attendee = _fakers.MeetingAttendee.Generate();
        attendee.Meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(attendee);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetingAttendees/{attendee.StringId}/relationships/meeting";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

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
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        Meeting meeting = _fakers.Meeting.Generate();
        meeting.Attendees = _fakers.MeetingAttendee.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Meetings.Add(meeting);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetings/{meeting.StringId}/relationships/attendees";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        string[] meetingIds = meeting.Attendees.Select(attendee => attendee.StringId!).OrderBy(id => id).ToArray();

        responseDocument.Should().BeJson(@"{
  ""links"": {
    ""self"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees"",
    ""related"": ""http://localhost/meetings/" + meeting.StringId + @"/attendees"",
    ""first"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees"",
    ""last"": ""http://localhost/meetings/" + meeting.StringId + @"/relationships/attendees""
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
  ],
  ""meta"": {
    ""total"": 2
  }
}");
    }

    [Fact]
    public async Task Can_create_resource_with_side_effects()
    {
        // Arrange
        Meeting newMeeting = _fakers.Meeting.Generate();
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

        const string route = "/meetings";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

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
      ""startTime"": """ + newMeeting.StartTime.ToString(JsonDateTimeOffsetFormatSpecifier) + @""",
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
        MeetingAttendee existingAttendee = _fakers.MeetingAttendee.Generate();

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

        string route = $"/meetingAttendees/{existingAttendee.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

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

    [Fact]
    public async Task Can_update_resource_with_relationship_for_type_at_end()
    {
        // Arrange
        MeetingAttendee existingAttendee = _fakers.MeetingAttendee.Generate();
        existingAttendee.Meeting = _fakers.Meeting.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(existingAttendee);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                id = existingAttendee.StringId,
                attributes = new
                {
                    displayName = existingAttendee.DisplayName
                },
                relationships = new
                {
                    meeting = new
                    {
                        data = new
                        {
                            id = existingAttendee.Meeting.StringId,
                            type = "meetings"
                        }
                    }
                },
                type = "meetingAttendees"
            }
        };

        string route = $"/meetingAttendees/{existingAttendee.StringId}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Includes_version_on_resource_endpoint()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeJsonApiVersion = true;

        MeetingAttendee attendee = _fakers.MeetingAttendee.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Attendees.Add(attendee);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/meetingAttendees/{attendee.StringId}/meeting";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().BeJson(@"{
  ""jsonapi"": {
    ""version"": ""1.1""
  },
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/" + attendee.StringId + @"/meeting""
  },
  ""data"": null
}");
    }

    [Fact]
    public async Task Includes_version_on_error_in_resource_endpoint()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeJsonApiVersion = true;

        string attendeeId = Unknown.StringId.For<MeetingAttendee, Guid>();

        string route = $"/meetingAttendees/{attendeeId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        string errorId = JsonApiStringConverter.ExtractErrorId(responseDocument);

        responseDocument.Should().BeJson(@"{
  ""jsonapi"": {
    ""version"": ""1.1""
  },
  ""links"": {
    ""self"": ""http://localhost/meetingAttendees/ffffffff-ffff-ffff-ffff-ffffffffffff""
  },
  ""errors"": [
    {
      ""id"": """ + errorId + @""",
      ""status"": ""404"",
      ""title"": ""The requested resource does not exist."",
      ""detail"": ""Resource of type 'meetingAttendees' with ID '" + attendeeId + @"' does not exist.""
    }
  ]
}");
    }
}
