using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Updating.Resources
{
    public sealed class AtomicUpdateResourceTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicUpdateResourceTests(ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_update_resources()
        {
            // Arrange
            const int elementCount = 5;

            var existingTracks = _fakers.MusicTrack.Generate(elementCount);
            var newTrackTitles = _fakers.MusicTrack.Generate(elementCount).Select(musicTrack => musicTrack.Title).ToArray();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var tracksInDatabase = await dbContext.MusicTracks
                    .ToListAsync();

                tracksInDatabase.Should().HaveCount(elementCount);

                for (int index = 0; index < elementCount; index++)
                {
                    var trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == existingTracks[index].Id);

                    trackInDatabase.Title.Should().Be(newTrackTitles[index]);
                    trackInDatabase.Genre.Should().Be(existingTracks[index].Genre);
                }
            });
        }

        [Fact]
        public async Task Can_update_resource_without_attributes_or_relationships()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.Genre.Should().Be(existingTrack.Genre);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_unknown_attribute()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            var newTitle = _fakers.MusicTrack.Generate().Title;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                trackInDatabase.Title.Should().Be(newTitle);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_unknown_relationship()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();

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
                                        type = "doesNotExist",
                                        id = 12345678
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_partially_update_resource_without_side_effects()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

            var newGenre = _fakers.MusicTrack.Generate().Genre;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.LengthInSeconds.Should().Be(existingTrack.LengthInSeconds);
                trackInDatabase.Genre.Should().Be(newGenre);
                trackInDatabase.ReleasedAt.Should().BeCloseTo(existingTrack.ReleasedAt);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
            });
        }

        [Fact]
        public async Task Can_completely_update_resource_without_side_effects()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

            var newTitle = _fakers.MusicTrack.Generate().Title;
            var newLengthInSeconds = _fakers.MusicTrack.Generate().LengthInSeconds;
            var newGenre = _fakers.MusicTrack.Generate().Genre;
            var newReleasedAt = _fakers.MusicTrack.Generate().ReleasedAt;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                trackInDatabase.Title.Should().Be(newTitle);
                trackInDatabase.LengthInSeconds.Should().Be(newLengthInSeconds);
                trackInDatabase.Genre.Should().Be(newGenre);
                trackInDatabase.ReleasedAt.Should().BeCloseTo(newReleasedAt);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_side_effects()
        {
            // Arrange
            var existingLanguage = _fakers.TextLanguage.Generate();
            var newIsoCode = _fakers.TextLanguage.Generate().IsoCode;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("textLanguages");
            responseDocument.Results[0].SingleData.Attributes["isoCode"].Should().Be(newIsoCode);
            responseDocument.Results[0].SingleData.Attributes.Should().NotContainKey("concurrencyToken");
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var languageInDatabase = await dbContext.TextLanguages
                    .FirstAsync(language => language.Id == existingLanguage.Id);

                languageInDatabase.IsoCode.Should().Be(newIsoCode);
            });
        }

        [Fact]
        public async Task Update_resource_with_side_effects_hides_relationship_data_in_response()
        {
            // Arrange
            var existingLanguage = _fakers.TextLanguage.Generate();
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[0].SingleData.Relationships.Values.Should().OnlyContain(relationshipEntry => relationshipEntry.Data == null);
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Usage of the 'href' element is not supported.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Can_update_resource_for_ref_element()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();
            var newArtistName = _fakers.Performer.Generate().ArtistName;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performerInDatabase = await dbContext.Performers
                    .FirstAsync(performer => performer.Id == existingPerformer.Id);

                performerInDatabase.ArtistName.Should().Be(newArtistName);
                performerInDatabase.BornAt.Should().BeCloseTo(existingPerformer.BornAt);
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
                            id = 12345678
                        },
                        data = new
                        {
                            type = "performers",
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'ref.type' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'ref.id' or 'ref.lid' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            id = 12345678,
                            lid = "local-1"
                        },
                        data = new
                        {
                            type = "performers",
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'ref.id' or 'ref.lid' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'data.type' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'data.id' or 'data.lid' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: The 'data.id' or 'data.lid' element is required.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_for_array_in_data()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Expected single data element for create/update resource operation.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            id = 12345678
                        },
                        data = new
                        {
                            type = "playlists",
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource type mismatch between 'ref.type' and 'data.type' element.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource of type 'performers' in 'data.type', instead of 'playlists'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_on_resource_ID_mismatch_between_ref_and_data()
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
                            id = 12345678
                        },
                        data = new
                        {
                            type = "performers",
                            id = 87654321,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource ID mismatch between 'ref.id' and 'data.id' element.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource with ID '12345678' in 'data.id', instead of '87654321'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource local ID mismatch between 'ref.lid' and 'data.lid' element.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource with local ID 'local-1' in 'data.lid', instead of 'local-2'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_on_mixture_of_ID_and_local_ID_between_ref_and_data()
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
                            id = "12345678"
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource identity mismatch between 'ref.id' and 'data.lid' element.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource with ID '12345678' in 'data.id', instead of 'local-1' in 'data.lid'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_on_mixture_of_local_ID_and_ID_between_ref_and_data()
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
                            id = "12345678",
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource identity mismatch between 'ref.lid' and 'data.id' element.");
            responseDocument.Errors[0].Detail.Should().Be("Expected resource with local ID 'local-1' in 'data.lid', instead of '12345678' in 'data.id'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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
                            type = "doesNotExist",
                            id = 12345678,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().Be("Resource type 'doesNotExist' does not exist.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_for_unknown_ID()
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
                            id = 99999999,
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'performers' with ID '99999999' does not exist.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_for_incompatible_ID()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().Be($"Failed to convert '{guid}' of type 'String' to type 'Int32'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_attribute_with_blocked_capability()
        {
            // Arrange
            var existingLyric = _fakers.Lyric.Generate();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Changing the value of the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Changing the value of 'createdAt' is not allowed.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_with_readonly_attribute()
        {
            // Arrange
            var existingPlaylist = _fakers.Playlist.Generate();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
            responseDocument.Errors[0].Detail.Should().Be("Attribute 'isArchived' is read-only.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_change_ID_of_existing_resource()
        {
            // Arrange
            var existingCompany = _fakers.RecordCompany.Generate();

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Resource ID is read-only.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_update_resource_with_incompatible_attribute_value()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();

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
                                bornAt = "not-a-valid-time"
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Failed to convert 'not-a-valid-time' of type 'String' to type 'DateTimeOffset'. - Request body:");
            responseDocument.Errors[0].Source.Pointer.Should().BeNull();
        }

        [Fact]
        public async Task Can_update_resource_with_attributes_and_multiple_relationship_types()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Lyric = _fakers.Lyric.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();
            existingTrack.Performers = _fakers.Performer.Generate(1);

            var newGenre = _fakers.MusicTrack.Generate().Genre;

            var existingLyric = _fakers.Lyric.Generate();
            var existingCompany = _fakers.RecordCompany.Generate();
            var existingPerformer = _fakers.Performer.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingTrack, existingLyric, existingCompany, existingPerformer);
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Lyric)
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.Genre.Should().Be(newGenre);

                trackInDatabase.Lyric.Should().NotBeNull();
                trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
            });
        }
    }
}
