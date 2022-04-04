using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Updating.Resources;

public sealed class AtomicUpdateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicUpdateResourceTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
        testContext.UseController<TextLanguagesController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddResourceDefinition<ImplicitlyChangingTextLanguageDefinition>();

            services.AddSingleton<ResourceDefinitionHitCounter>();
        });

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = false;
    }

    [Fact]
    public async Task Can_update_resources()
    {
        // Arrange
        const int elementCount = 5;

        List<MusicTrack> existingTracks = _fakers.MusicTrack.Generate(elementCount);
        string[] newTrackTitles = _fakers.MusicTrack.Generate(elementCount).Select(musicTrack => musicTrack.Title).ToArray();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<MusicTrack>();
            dbContext.MusicTracks.AddRange(existingTracks);
            await dbContext.SaveChangesAsync();
        });

        var operationElements = new List<object>(elementCount);

        for (int index = 0; index < elementCount; index++)
        {
            operationElements.Add(new
            {
                op = "update",
                data = new
                {
                    type = "musicTracks",
                    id = existingTracks[index].StringId,
                    attributes = new
                    {
                        title = newTrackTitles[index]
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();

            tracksInDatabase.ShouldHaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                MusicTrack trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == existingTracks[index].Id);

                trackInDatabase.Title.Should().Be(newTrackTitles[index]);
                trackInDatabase.Genre.Should().Be(existingTracks[index].Genre);
            }
        });
    }

    [Fact]
    public async Task Can_update_resource_without_attributes_or_relationships()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

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
                        },
                        relationships = new
                        {
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

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.Genre.Should().Be(existingTrack.Genre);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_attribute()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        string newTitle = _fakers.MusicTrack.Generate().Title;

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
                            title = newTitle,
                            doesNotExist = "Ignored"
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
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'doesNotExist' does not exist on resource type 'musicTracks'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/doesNotExist");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_update_resource_with_unknown_attribute()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        string newTitle = _fakers.MusicTrack.Generate().Title;

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
                            title = newTitle,
                            doesNotExist = "Ignored"
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

            trackInDatabase.Title.Should().Be(newTitle);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_with_unknown_relationship()
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown relationship found.");
        error.Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource type 'musicTracks'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/doesNotExist");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_update_resource_with_unknown_relationship()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_partially_update_resource_without_side_effects()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

        string newGenre = _fakers.MusicTrack.Generate().Genre!;

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
                            genre = newGenre
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

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.LengthInSeconds.Should().BeApproximately(existingTrack.LengthInSeconds);
            trackInDatabase.Genre.Should().Be(newGenre);
            trackInDatabase.ReleasedAt.Should().Be(existingTrack.ReleasedAt);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
        });
    }

    [Fact]
    public async Task Can_completely_update_resource_without_side_effects()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

        string newTitle = _fakers.MusicTrack.Generate().Title;
        decimal? newLengthInSeconds = _fakers.MusicTrack.Generate().LengthInSeconds;
        string newGenre = _fakers.MusicTrack.Generate().Genre!;
        DateTimeOffset newReleasedAt = _fakers.MusicTrack.Generate().ReleasedAt;

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
                            title = newTitle,
                            lengthInSeconds = newLengthInSeconds,
                            genre = newGenre,
                            releasedAt = newReleasedAt
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

            trackInDatabase.Title.Should().Be(newTitle);
            trackInDatabase.LengthInSeconds.Should().BeApproximately(newLengthInSeconds);
            trackInDatabase.Genre.Should().Be(newGenre);
            trackInDatabase.ReleasedAt.Should().Be(newReleasedAt);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_side_effects()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.Generate();
        string newIsoCode = _fakers.TextLanguage.Generate().IsoCode!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextLanguages.Add(existingLanguage);
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
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        attributes = new
                        {
                            isoCode = newIsoCode
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

        string isoCode = $"{newIsoCode}{ImplicitlyChangingTextLanguageDefinition.Suffix}";

        responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
        {
            resource.Type.Should().Be("textLanguages");
            resource.Attributes.ShouldContainKey("isoCode").With(value => value.Should().Be(isoCode));
            resource.Attributes.Should().NotContainKey("isRightToLeft");
            resource.Relationships.ShouldNotBeEmpty();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextLanguage languageInDatabase = await dbContext.TextLanguages.FirstWithIdAsync(existingLanguage.Id);
            languageInDatabase.IsoCode.Should().Be(isoCode);
        });
    }

    [Fact]
    public async Task Update_resource_with_side_effects_hides_relationship_data_in_response()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.Generate();
        existingLanguage.Lyrics = _fakers.Lyric.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextLanguages.Add(existingLanguage);
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
                        type = "textLanguages",
                        id = existingLanguage.StringId
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
            resource.Relationships.ShouldNotBeEmpty();
            resource.Relationships.Values.Should().OnlyContain(value => value != null && value.Data.Value == null);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_for_href_element()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    href = "/api/v1/musicTracks/1"
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
        error.Title.Should().Be("Failed to deserialize request body: The 'href' element is not supported.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/href");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_update_resource_for_ref_element()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();
        string newArtistName = _fakers.Performer.Generate().ArtistName!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingPerformer);
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
                        type = "performers",
                        id = existingPerformer.StringId
                    },
                    data = new
                    {
                        type = "performers",
                        id = existingPerformer.StringId,
                        attributes = new
                        {
                            artistName = newArtistName
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
            Performer performerInDatabase = await dbContext.Performers.FirstWithIdAsync(existingPerformer.Id);

            performerInDatabase.ArtistName.Should().Be(newArtistName);
            performerInDatabase.BornAt.Should().Be(existingPerformer.BornAt);
        });
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_type_in_ref()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        id = Unknown.StringId.For<Performer, int>()
                    },
                    data = new
                    {
                        type = "performers",
                        id = Unknown.StringId.For<Performer, int>(),
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_ID_in_ref()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers"
                    },
                    data = new
                    {
                        type = "performers",
                        id = Unknown.StringId.For<Performer, int>(),
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_ID_and_local_ID_in_ref()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        id = Unknown.StringId.For<Performer, int>(),
                        lid = "local-1"
                    },
                    data = new
                    {
                        type = "performers",
                        id = Unknown.StringId.AltFor<Performer, int>(),
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update"
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_null_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = (object?)null
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
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_array_data()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingPerformer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new[]
                    {
                        new
                        {
                            type = "performers",
                            id = existingPerformer.StringId,
                            attributes = new
                            {
                                artistName = existingPerformer.ArtistName
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
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_type_in_data()
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
                        id = Unknown.StringId.Int32,
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_missing_ID_in_data()
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
                        type = "performers",
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_ID_and_local_ID_in_data()
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
                        type = "performers",
                        id = Unknown.StringId.For<Performer, int>(),
                        lid = "local-1",
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_on_resource_type_mismatch_between_ref_and_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        id = Unknown.StringId.For<Performer, int>()
                    },
                    data = new
                    {
                        type = "playlists",
                        id = Unknown.StringId.For<Playlist, long>(),
                        attributes = new
                        {
                        },
                        relationships = new
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'playlists' is not convertible to type 'performers'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_on_resource_ID_mismatch_between_ref_and_data()
    {
        // Arrange
        string performerId1 = Unknown.StringId.For<Performer, int>();
        string performerId2 = Unknown.StringId.AltFor<Performer, int>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        id = performerId1
                    },
                    data = new
                    {
                        type = "performers",
                        id = performerId2,
                        attributes = new
                        {
                        },
                        relationships = new
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Conflicting 'id' values found.");
        error.Detail.Should().Be($"Expected '{performerId1}' instead of '{performerId2}'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_on_resource_local_ID_mismatch_between_ref_and_data()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        lid = "local-1"
                    },
                    data = new
                    {
                        type = "performers",
                        lid = "local-2",
                        attributes = new
                        {
                        },
                        relationships = new
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Conflicting 'lid' values found.");
        error.Detail.Should().Be("Expected 'local-1' instead of 'local-2'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/lid");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_on_mixture_of_ID_and_local_ID_between_ref_and_data()
    {
        // Arrange
        string performerId = Unknown.StringId.For<Performer, int>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        id = performerId
                    },
                    data = new
                    {
                        type = "performers",
                        lid = "local-1",
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_on_mixture_of_local_ID_and_ID_between_ref_and_data()
    {
        // Arrange
        string performerId = Unknown.StringId.For<Performer, int>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        lid = "local-1"
                    },
                    data = new
                    {
                        type = "performers",
                        id = performerId,
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'lid' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_unknown_type()
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
                        type = Unknown.ResourceType,
                        id = Unknown.StringId.Int32,
                        attributes = new
                        {
                        },
                        relationships = new
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_for_unknown_ID()
    {
        // Arrange
        string performerId = Unknown.StringId.For<Performer, int>();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "performers",
                        id = performerId,
                        attributes = new
                        {
                        },
                        relationships = new
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'performers' with ID '{performerId}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_update_resource_for_incompatible_ID()
    {
        // Arrange
        string guid = Unknown.StringId.Guid;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "performers",
                        id = guid
                    },
                    data = new
                    {
                        type = "performers",
                        id = guid,
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible 'id' value found.");
        error.Detail.Should().Be($"Failed to convert '{guid}' of type 'String' to type 'Int32'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_attribute_with_blocked_capability()
    {
        // Arrange
        Lyric existingLyric = _fakers.Lyric.Generate();

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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute value cannot be assigned when updating resource.");
        error.Detail.Should().Be("The attribute 'createdAt' on resource type 'lyrics' cannot be assigned to.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/createdAt");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_with_readonly_attribute()
    {
        // Arrange
        Playlist existingPlaylist = _fakers.Playlist.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Playlists.Add(existingPlaylist);
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
                        attributes = new
                        {
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

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        error.Detail.Should().Be("Attribute 'isArchived' on resource type 'playlists' is read-only.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/isArchived");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_change_ID_of_existing_resource()
    {
        // Arrange
        RecordCompany existingCompany = _fakers.RecordCompany.Generate();

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
                    op = "update",
                    data = new
                    {
                        type = "recordCompanies",
                        id = existingCompany.StringId,
                        attributes = new
                        {
                            id = (existingCompany.Id + 1).ToString()
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
        error.Title.Should().Be("Failed to deserialize request body: Resource ID is read-only.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_update_resource_with_incompatible_attribute_value()
    {
        // Arrange
        Performer existingPerformer = _fakers.Performer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Performers.Add(existingPerformer);
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
                        type = "performers",
                        id = existingPerformer.StringId,
                        attributes = new
                        {
                            bornAt = 123.45
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
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Detail.Should().Be("Failed to convert attribute 'bornAt' with value '123.45' of type 'Number' to type 'DateTimeOffset'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/bornAt");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_update_resource_with_attributes_and_multiple_relationship_types()
    {
        // Arrange
        MusicTrack existingTrack = _fakers.MusicTrack.Generate();
        existingTrack.Lyric = _fakers.Lyric.Generate();
        existingTrack.OwnedBy = _fakers.RecordCompany.Generate();
        existingTrack.Performers = _fakers.Performer.Generate(1);

        string newGenre = _fakers.MusicTrack.Generate().Genre!;

        Lyric existingLyric = _fakers.Lyric.Generate();
        RecordCompany existingCompany = _fakers.RecordCompany.Generate();
        Performer existingPerformer = _fakers.Performer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingTrack, existingLyric, existingCompany, existingPerformer);
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
                            genre = newGenre
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
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            MusicTrack trackInDatabase = await dbContext.MusicTracks
                .Include(musicTrack => musicTrack.Lyric)
                .Include(musicTrack => musicTrack.OwnedBy)
                .Include(musicTrack => musicTrack.Performers)
                .FirstWithIdAsync(existingTrack.Id);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            trackInDatabase.Title.Should().Be(existingTrack.Title);
            trackInDatabase.Genre.Should().Be(newGenre);

            trackInDatabase.Lyric.ShouldNotBeNull();
            trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);

            trackInDatabase.OwnedBy.ShouldNotBeNull();
            trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);

            trackInDatabase.Performers.ShouldHaveCount(1);
            trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
        });
    }
}
