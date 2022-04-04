using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Creating;

public sealed class AtomicCreateResourceWithToOneRelationshipTests
    : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceWithToOneRelationshipTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
        testContext.UseController<MusicTracksController>();
        testContext.UseController<LyricsController>();
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();

        string newLyricText = _fakers.Lyric.Generate().Text;

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
                        type = "lyrics",
                        attributes = new
                        {
                            text = newLyricText
                        },
                        relationships = new
                        {
                            track = new
                            {
                                data = new
                                {
                                    type = "musicTracks",
                                    id = existingTrack.StringId
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

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("lyrics");
            resource.Attributes.ShouldNotBeEmpty();
            resource.Relationships.ShouldNotBeEmpty();
        });

        long newLyricId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(newLyricId);

            lyricInDatabase.Track.ShouldNotBeNull();
            lyricInDatabase.Track.Id.Should().Be(existingTrack.Id);
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.Generate();
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Lyrics.Add(existingLyric);
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
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTrackTitle
                        },
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "lyrics",
                                    id = existingLyric.StringId
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

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Attributes.ShouldNotBeEmpty();
            resource.Relationships.ShouldNotBeEmpty();
        });

        Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(newTrackId);

            trackInDatabase.Lyric.ShouldNotBeNull();
            trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);
        });
    }

    [Fact]
    public async Task Can_create_resources_with_ToOne_relationship()
    {
        // Arrange
        const int elementCount = 5;

        RecordCompany existingCompany = _fakers.RecordCompany.Generate();
        string[] newTrackTitles = _fakers.MusicTrack.Generate(elementCount).Select(musicTrack => musicTrack.Title).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RecordCompanies.Add(existingCompany);
            await dbContext.SaveChangesAsync();
        });

        var operationElements = new List<object>(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            operationElements.Add(new
            {
                op = "add",
                data = new
                {
                    type = "musicTracks",
                    attributes = new
                    {
                        title = newTrackTitles[index]
                    },
                    relationships = new
                    {
                        ownedBy = new
                        {
                            data = new
                            {
                                type = "recordCompanies",
                                id = existingCompany.StringId
                            }
                        }
                    }
                }
            });
        }

        var requestBody = new
        {
            atomic__operations = operationElements
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            responseDocument.Results[index].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitles[index]));
            });
        }

        Guid[] newTrackIds = responseDocument.Results.Select(result => Guid.Parse(result.Data.SingleValue!.Id.ShouldNotBeNull())).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks
                .Include(musicTrack => musicTrack.OwnedBy)
                .Where(musicTrack => newTrackIds.Contains(musicTrack.Id))
                .ToListAsync();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            tracksInDatabase.ShouldHaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == newTrackIds[index]);

                trackInDatabase.Title.Should().Be(newTrackTitles[index]);

                trackInDatabase.OwnedBy.ShouldNotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
            }
        });
    }

    [Fact]
    public async Task Cannot_create_for_null_relationship()
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
                        relationships = new
                        {
                            lyric = (object?)null
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
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_for_missing_data_in_relationship()
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
                        relationships = new
                        {
                            lyric = new
                            {
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_for_array_data_in_relationship()
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "lyrics",
                                        id = Unknown.StringId.For<Lyric, long>()
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null', instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_for_missing_relationship_type()
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    id = Unknown.StringId.For<Lyric, long>()
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_for_unknown_relationship_type()
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = Unknown.ResourceType,
                                    id = Unknown.StringId.For<Lyric, long>()
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_for_missing_relationship_ID()
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "lyrics"
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_with_unknown_relationship_ID()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        string lyricId = Unknown.StringId.For<Lyric, long>();

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
                            title = newTrackTitle
                        },
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "lyrics",
                                    id = lyricId
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'lyrics' with ID '{lyricId}' in relationship 'lyric' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_create_on_relationship_type_mismatch()
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "playlists",
                                    id = Unknown.StringId.For<Playlist, long>()
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'playlists' is not convertible to type 'lyrics' of relationship 'lyric'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_create_resource_with_duplicate_relationship()
    {
        // Arrange
        RecordCompany existingCompany = _fakers.RecordCompany.Generate();
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RecordCompanies.Add(existingCompany);
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
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTrackTitle
                        },
                        relationships = new
                        {
                            ownedBy = new
                            {
                                data = new
                                {
                                    type = "recordCompanies",
                                    id = existingCompany.StringId
                                }
                            },
                            ownedBy_duplicate = new
                            {
                                data = new
                                {
                                    type = "recordCompanies",
                                    id = existingCompany.StringId
                                }
                            }
                        }
                    }
                }
            }
        };

        string requestBodyText = JsonSerializer.Serialize(requestBody).Replace("ownedBy_duplicate", "ownedBy");

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBodyText);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.ShouldHaveCount(1);

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Attributes.ShouldNotBeEmpty();
            resource.Relationships.ShouldNotBeEmpty();
        });

        Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(newTrackId);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
        });
    }
}
