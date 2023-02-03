using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ModelStateValidation;

public sealed class AtomicModelStateValidationTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicModelStateValidationTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        _testContext.ConfigureServicesBeforeStartup(services =>
        {
            services.AddSingleton<ISystemClock, FrozenSystemClock>();
        });

        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_with_multiple_violations()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            lengthInSeconds = -1
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error1.Title.Should().Be("Input validation failed.");
        error1.Detail.Should().Be("The Title field is required.");
        error1.Source.ShouldNotBeNull();
        error1.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/title");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error2.Title.Should().Be("Input validation failed.");
        error2.Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/lengthInSeconds");
    }

    [Fact]
    public async Task Cannot_create_resource_when_violation_from_custom_ValidationAttribute()
    {
        // Arrange
        var clock = _testContext.Factory.Services.GetRequiredService<ISystemClock>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = "some",
                            lengthInSeconds = 120,
                            releasedAt = clock.UtcNow.AddDays(1)
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("ReleasedAt must be in the past.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/releasedAt");
    }

    [Fact]
    public async Task Can_create_resource_with_annotated_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        string newPlaylistName = _fakers.Playlist.Generate().Name;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = newPlaylistName
                        },
                        relationships = new
                        {
                            tracks = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "musicTracks",
                                        id = existingTrack.StringId
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(1);

        long newPlaylistId = long.Parse(responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(newPlaylistId);

            playlistInDatabase.Tracks.ShouldHaveCount(1);
            playlistInDatabase.Tracks[0].Id.Should().Be(existingTrack.Id);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_with_multiple_violations()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                            title = (string?)null,
                            lengthInSeconds = -1
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error1.Title.Should().Be("Input validation failed.");
        error1.Detail.Should().Be("The Title field is required.");
        error1.Source.ShouldNotBeNull();
        error1.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/title");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error2.Title.Should().Be("Input validation failed.");
        error2.Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/lengthInSeconds");
    }

    [Fact]
    public async Task Can_update_resource_with_omitted_required_attribute()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        string newTrackGenre = _fakers.MusicTrack.Generate().Genre!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        attributes = new
                        {
                            genre = newTrackGenre
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.Genre.Should().Be(newTrackGenre);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_annotated_relationship()
    {
        // Arrange
        Playlist existingPlaylist = _fakers.Playlist.Generate();
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPlaylist, existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "playlists",
                        id = existingPlaylist.StringId,
                        relationships = new
                        {
                            tracks = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "musicTracks",
                                        id = existingTrack.StringId
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(existingPlaylist.Id);

            playlistInDatabase.Tracks.ShouldHaveCount(1);
            playlistInDatabase.Tracks[0].Id.Should().Be(existingTrack.Id);
        });
    }

    [Fact]
    public async Task Can_update_ManyToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        RecordCompany existingCompany = _fakers.RecordCompany.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTrack, existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
                        relationship = "ownedBy"
                    },
                    data = new
                    {
                        type = "recordCompanies",
                        id = existingCompany.StringId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
        });
    }

    [Fact]
    public async Task Can_update_ManyToMany_relationship()
    {
        // Arrange
        Playlist existingPlaylist = _fakers.Playlist.Generate();
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPlaylist, existingTrack);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "playlists",
                        id = existingPlaylist.StringId,
                        relationship = "tracks"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(existingPlaylist.Id);

            playlistInDatabase.Tracks.ShouldHaveCount(1);
            playlistInDatabase.Tracks[0].Id.Should().Be(existingTrack.Id);
        });
    }

    [Fact]
    public async Task Validates_all_operations_before_execution_starts()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "playlists",
                        id = Unknown.StringId.For<Playlist, long>(),
                        attributes = new
                        {
                            name = (string?)null
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = "some",
                            lengthInSeconds = -1
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error1.Title.Should().Be("Input validation failed.");
        error1.Detail.Should().Be("The Name field is required.");
        error1.Source.ShouldNotBeNull();
        error1.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/name");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error2.Title.Should().Be("Input validation failed.");
        error2.Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/atomic:operations[1]/data/attributes/lengthInSeconds");
    }

    [Fact]
    public async Task Does_not_exceed_MaxModelValidationErrors()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = (string?)null
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = (string?)null
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            lengthInSeconds = -1
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "playlists",
                        attributes = new
                        {
                            name = (string?)null
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(3);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error1.Title.Should().Be("Input validation failed.");
        error1.Detail.Should().Be("The maximum number of allowed model errors has been reached.");
        error1.Source.Should().BeNull();

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error2.Title.Should().Be("Input validation failed.");
        error2.Detail.Should().Be("The Name field is required.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/name");

        ErrorObject error3 = responseDocument.Errors[2];
        error3.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error3.Title.Should().Be("Input validation failed.");
        error3.Detail.Should().Be("The Name field is required.");
        error3.Source.ShouldNotBeNull();
        error3.Source.Pointer.Should().Be("/atomic:operations[1]/data/attributes/name");
    }
}
