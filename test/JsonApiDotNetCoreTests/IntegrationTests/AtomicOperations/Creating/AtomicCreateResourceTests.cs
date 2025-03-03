using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Creating;

public sealed class AtomicCreateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicCreateResourceTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
        testContext.UseController<LyricsController>();
        testContext.UseController<MusicTracksController>();
        testContext.UseController<PlaylistsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = false;
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        string newArtistName = _fakers.Performer.GenerateOne().ArtistName!;
        DateTimeOffset newBornAt = _fakers.Performer.GenerateOne().BornAt;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "performers",
                        attributes = new
                        {
                            artistName = newArtistName,
                            bornAt = newBornAt
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

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("performers");
            resource.Attributes.Should().ContainKey("artistName").WhoseValue.Should().Be(newArtistName);
            resource.Attributes.Should().ContainKey("bornAt").WhoseValue.Should().Be(newBornAt);
            resource.Relationships.Should().BeNull();
        });

        int newPerformerId = int.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(newPerformerId);

            performerInDatabase.ArtistName.Should().Be(newArtistName);
            performerInDatabase.BornAt.Should().Be(newBornAt);
        });
    }

    [Fact]
    public async Task Can_create_resources()
    {
        // Arrange
        const int elementCount = 5;

        List<MusicTrack> newTracks = _fakers.MusicTrack.GenerateList(elementCount);

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
                        title = newTracks[index].Title,
                        lengthInSeconds = newTracks[index].LengthInSeconds,
                        genre = newTracks[index].Genre,
                        releasedAt = newTracks[index].ReleasedAt
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

        responseDocument.Results.Should().HaveCount(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            responseDocument.Results[index].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
            {
                resource.Should().NotBeNull();
                resource.Type.Should().Be("musicTracks");
                resource.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(newTracks[index].Title);
                resource.Attributes.Should().ContainKey("lengthInSeconds").WhoseValue.As<decimal?>().Should().BeApproximately(newTracks[index].LengthInSeconds);
                resource.Attributes.Should().ContainKey("genre").WhoseValue.Should().Be(newTracks[index].Genre);
                resource.Attributes.Should().ContainKey("releasedAt").WhoseValue.Should().Be(newTracks[index].ReleasedAt);

                resource.Relationships.Should().NotBeEmpty();
            });
        }

        Guid[] newTrackIds = responseDocument.Results.Select(result => Guid.Parse(result.Data.SingleValue!.Id.Should().NotBeNull().And.Subject)).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.Where(musicTrack => newTrackIds.Contains(musicTrack.Id)).ToListAsync();

            tracksInDatabase.Should().HaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == newTrackIds[index]);

                trackInDatabase.Title.Should().Be(newTracks[index].Title);
                trackInDatabase.LengthInSeconds.Should().BeApproximately(newTracks[index].LengthInSeconds);
                trackInDatabase.Genre.Should().Be(newTracks[index].Genre);
                trackInDatabase.ReleasedAt.Should().Be(newTracks[index].ReleasedAt);
            }
        });
    }

    [Fact]
    public async Task Can_create_resource_without_attributes_or_relationships()
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
                        type = "performers",
                        attributes = new
                        {
                        },
                        relationship = new
                        {
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

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("performers");
            resource.Attributes.Should().ContainKey("artistName").WhoseValue.Should().BeNull();
            resource.Attributes.Should().ContainKey("bornAt").WhoseValue.Should().Be(default(DateTimeOffset));
            resource.Relationships.Should().BeNull();
        });

        int newPerformerId = int.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(newPerformerId);

            performerInDatabase.ArtistName.Should().BeNull();
            performerInDatabase.BornAt.Should().Be(default);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_attribute()
    {
        // Arrange
        string newName = _fakers.Playlist.GenerateOne().Name;

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
                            doesNotExist = "ignored",
                            name = newName
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
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'doesNotExist' does not exist on resource type 'playlists'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/doesNotExist");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_create_resource_with_unknown_attribute()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        string newName = _fakers.Playlist.GenerateOne().Name;

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
                            doesNotExist = "ignored",
                            name = newName
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

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("playlists");
            resource.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(newName);
            resource.Relationships.Should().NotBeEmpty();
        });

        long newPlaylistId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Playlist performerInDatabase = await dbContext.Playlists.FirstWithIdAsync(newPlaylistId);

            performerInDatabase.Name.Should().Be(newName);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_relationship()
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
                        type = "lyrics",
                        relationships = new
                        {
                            doesNotExist = new
                            {
                                data = new
                                {
                                    type = Unknown.ResourceType,
                                    id = Unknown.StringId.Int32
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
        error.Title.Should().Be("Failed to deserialize request body: Unknown relationship found.");
        error.Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource type 'lyrics'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/doesNotExist");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_create_resource_with_unknown_relationship()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        string newLyricText = _fakers.Lyric.GenerateOne().Text;

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
                            doesNotExist = new
                            {
                                data = new
                                {
                                    type = Unknown.ResourceType,
                                    id = Unknown.StringId.Int32
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

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("lyrics");
            resource.Attributes.Should().NotBeEmpty();
            resource.Relationships.Should().NotBeEmpty();
        });

        long newLyricId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Lyric lyricInDatabase = await dbContext.Lyrics.FirstWithIdAsync(newLyricId);

            lyricInDatabase.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_client_generated_ID()
    {
        // Arrange
        MusicTrack newTrack = _fakers.MusicTrack.GenerateOne();
        newTrack.Id = Guid.NewGuid();

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
                        id = newTrack.StringId,
                        attributes = new
                        {
                            title = newTrack.Title
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_href_element()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    href = "/api/musicTracks"
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
        error.Title.Should().Be("Failed to deserialize request body: The 'href' element is not supported.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/href");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_ref_element()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    @ref = new
                    {
                        type = "musicTracks"
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
        error.Title.Should().Be("Failed to deserialize request body: The 'relationship' element is required.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_missing_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add"
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_null_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = (object?)null
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_array_data()
    {
        // Arrange
        string newArtistName = _fakers.Performer.GenerateOne().ArtistName!;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new[]
                    {
                        new
                        {
                            type = "performers",
                            attributes = new
                            {
                                artistName = newArtistName
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
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_missing_type()
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
                        attributes = new
                        {
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_for_unknown_type()
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
                        type = Unknown.ResourceType
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/type");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_with_readonly_attribute()
    {
        // Arrange
        string newPlaylistName = _fakers.Playlist.GenerateOne().Name;

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
                            name = newPlaylistName,
                            isArchived = true
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
        error.Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        error.Detail.Should().Be("Attribute 'isArchived' on resource type 'playlists' is read-only.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/isArchived");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Cannot_create_resource_with_incompatible_attribute_value()
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
                        type = "performers",
                        attributes = new
                        {
                            bornAt = 12345
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
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Detail.Should().Be("Failed to convert attribute 'bornAt' with value '12345' of type 'Number' to type 'DateTimeOffset'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/bornAt");
        error.Meta.Should().HaveRequestBody();
    }

    [Fact]
    public async Task Can_create_resource_with_attributes_and_multiple_relationship_types()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.GenerateOne();
        RecordCompany existingCompany = _fakers.RecordCompany.GenerateOne();
        Performer existingPerformer = _fakers.Performer.GenerateOne();

        string newTitle = _fakers.MusicTrack.GenerateOne().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLyric, existingCompany, existingPerformer);
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
                            title = newTitle
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
                            },
                            ownedBy = new
                            {
                                data = new
                                {
                                    type = "recordCompanies",
                                    id = existingCompany.StringId
                                }
                            },
                            performers = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "performers",
                                        id = existingPerformer.StringId
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

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("musicTracks");
            resource.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(newTitle);
            resource.Relationships.Should().NotBeEmpty();
        });

        Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_after_property_in_chained_method_calls true

            MusicTrack trackInDatabase = await dbContext.MusicTracks
                .Include(musicTrack => musicTrack.Lyric)
                .Include(musicTrack => musicTrack.OwnedBy)
                .Include(musicTrack => musicTrack.Performers)
                .FirstWithIdAsync(newTrackId);

            // @formatter:wrap_after_property_in_chained_method_calls restore
            // @formatter:wrap_chained_method_calls restore

            trackInDatabase.Title.Should().Be(newTitle);

            trackInDatabase.Lyric.Should().NotBeNull();
            trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);

            trackInDatabase.OwnedBy.Should().NotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);

            trackInDatabase.Performers.Should().HaveCount(1);
            trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
        });
    }

    [Fact]
    public async Task Cannot_assign_attribute_with_blocked_capability()
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
                        type = "lyrics",
                        attributes = new
                        {
                            createdAt = 12.July(1980)
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
        error.Title.Should().Be("Failed to deserialize request body: Attribute value cannot be assigned when creating resource.");
        error.Detail.Should().Be("The attribute 'createdAt' on resource type 'lyrics' cannot be assigned to.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/createdAt");
        error.Meta.Should().HaveRequestBody();
    }
}
