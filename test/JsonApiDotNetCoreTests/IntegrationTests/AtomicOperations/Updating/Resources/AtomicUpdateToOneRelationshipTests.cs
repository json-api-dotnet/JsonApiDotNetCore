using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Updating.Resources;

public sealed class AtomicUpdateToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicUpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.GenerateOne();
        existingLyric.Track = _fakers.MusicTrack.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<MusicTrack>();
            dbContext.Lyrics.Add(existingLyric);
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
                        type = "lyrics",
                        id = existingLyric.StringId,
                        relationships = new
                        {
                            track = new
                            {
                                data = (object?)null
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
            Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(existingLyric.Id);

            lyricInDatabase.Track.Should().BeNull();

            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
            tracksInDatabase.Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task Can_clear_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.Lyric = _fakers.Lyric.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Lyric>();
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
                        relationships = new
                        {
                            lyric = new
                            {
                                data = (object?)null
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
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Lyric.Should().BeNull();

            List<Lyric> lyricsInDatabase = await dbContext.Lyrics.ToListAsync();
            lyricsInDatabase.Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task Can_clear_ManyToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.OwnedBy = _fakers.RecordCompany.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RecordCompany>();
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
                        relationships = new
                        {
                            ownedBy = new
                            {
                                data = (object?)null
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
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.OwnedBy.Should().BeNull();

            List<RecordCompany> companiesInDatabase = await dbContext.RecordCompanies.ToListAsync();
            companiesInDatabase.Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.GenerateOne();
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLyric, existingTrack);
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
                        type = "lyrics",
                        id = existingLyric.StringId,
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(existingLyric.Id);

            lyricInDatabase.Track.Should().NotBeNull();
            lyricInDatabase.Track.Id.Should().Be(existingTrack.Id);
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        Lyric existingLyric = _fakers.Lyric.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTrack, existingLyric);
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Lyric.Should().NotBeNull();
            trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);
        });
    }

    [Fact]
    public async Task Can_create_ManyToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        RecordCompany existingCompany = _fakers.RecordCompany.GenerateOne();

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
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
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

            trackInDatabase.OwnedBy.Should().NotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.GenerateOne();
        existingLyric.Track = _fakers.MusicTrack.GenerateOne();

        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<MusicTrack>();
            dbContext.AddInRange(existingLyric, existingTrack);
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
                        type = "lyrics",
                        id = existingLyric.StringId,
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(existingLyric.Id);

            lyricInDatabase.Track.Should().NotBeNull();
            lyricInDatabase.Track.Id.Should().Be(existingTrack.Id);

            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
            tracksInDatabase.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.Lyric = _fakers.Lyric.GenerateOne();

        Lyric existingLyric = _fakers.Lyric.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Lyric>();
            dbContext.AddInRange(existingTrack, existingLyric);
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(existingTrack.Id);

            trackInDatabase.Lyric.Should().NotBeNull();
            trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);

            List<Lyric> lyricsInDatabase = await dbContext.Lyrics.ToListAsync();
            lyricsInDatabase.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Can_replace_ManyToOne_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();
        existingTrack.OwnedBy = _fakers.RecordCompany.GenerateOne();

        RecordCompany existingCompany = _fakers.RecordCompany.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RecordCompany>();
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
                    data = new
                    {
                        type = "musicTracks",
                        id = existingTrack.StringId,
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

            trackInDatabase.OwnedBy.Should().NotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);

            List<RecordCompany> companiesInDatabase = await dbContext.RecordCompanies.ToListAsync();
            companiesInDatabase.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Cannot_create_for_null_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_missing_data_in_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_array_data_in_relationship()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null', instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_missing_type_in_relationship_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "lyrics",
                        id = Unknown.StringId.For<Lyric, long>(),
                        relationships = new
                        {
                            track = new
                            {
                                data = new
                                {
                                    id = Unknown.StringId.For<MusicTrack, Guid>()
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/track/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_unknown_type_in_relationship_data()
    {
        // Arrange
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
                        id = Unknown.StringId.For<MusicTrack, Guid>(),
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_missing_ID_in_relationship_data()
    {
        // Arrange
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
                        id = Unknown.StringId.For<MusicTrack, Guid>(),
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_ID_and_local_ID_in_relationship_data()
    {
        // Arrange
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
                        id = Unknown.StringId.For<MusicTrack, Guid>(),
                        relationships = new
                        {
                            lyric = new
                            {
                                data = new
                                {
                                    type = "lyrics",
                                    id = Unknown.StringId.For<Lyric, long>(),
                                    lid = "local-1"
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_for_unknown_ID_in_relationship_data()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.MusicTracks.Add(existingTrack);
            await dbContext.SaveChangesAsync();
        });

        string lyricId = Unknown.StringId.For<Lyric, long>();

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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'lyrics' with ID '{lyricId}' in relationship 'lyric' does not exist.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_create_for_relationship_mismatch()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.GenerateOne();

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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'playlists' is not convertible to type 'lyrics' of relationship 'lyric'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/lyric/data/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_assign_relationship_with_blocked_capability()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.GenerateOne();

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
                    op = "update",
                    data = new
                    {
                        type = "lyrics",
                        id = existingLyric.StringId,
                        relationships = new
                        {
                            language = new
                            {
                                data = new
                                {
                                    type = "textLanguages",
                                    id = Unknown.StringId.For<TextLanguage, Guid>()
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Relationship cannot be assigned.");
        error.Detail.Should().Be("The relationship 'language' on resource type 'lyrics' cannot be assigned to.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/language");
        error.Meta.Should().HaveRequestBody();
    }
}
