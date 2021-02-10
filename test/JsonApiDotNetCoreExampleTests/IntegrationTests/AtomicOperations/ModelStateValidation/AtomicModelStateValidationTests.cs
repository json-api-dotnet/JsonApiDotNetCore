using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.ModelStateValidation
{
    public sealed class AtomicModelStateValidationTests
        : IClassFixture<ExampleIntegrationTestContext<ModelStateValidationStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<ModelStateValidationStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicModelStateValidationTests(ExampleIntegrationTestContext<ModelStateValidationStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(2);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The Title field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/title");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[1].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[1].Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
            responseDocument.Errors[1].Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/lengthInSeconds");
        }

        [Fact]
        public async Task Can_create_resource_with_annotated_relationship()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            var newPlaylistName = _fakers.Playlist.Generate().Name;

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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);

            var newPlaylistId = long.Parse(responseDocument.Results[0].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == newPlaylistId);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(existingTrack.Id);
            });
        }

        [Fact]
        public async Task Cannot_update_resource_with_multiple_violations()
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
                            attributes = new
                            {
                                title = (string) null,
                                lengthInSeconds = -1
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

            responseDocument.Errors.Should().HaveCount(2);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The Title field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/title");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[1].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[1].Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
            responseDocument.Errors[1].Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/lengthInSeconds");
        }

        [Fact]
        public async Task Can_update_resource_with_omitted_required_attribute()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            var newTrackGenre = _fakers.MusicTrack.Generate().Genre;

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

                trackInDatabase.Title.Should().Be(existingTrack.Title);
                trackInDatabase.Genre.Should().Be(newTrackGenre);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_annotated_relationship()
        {
            // Arrange
            var existingPlaylist = _fakers.Playlist.Generate();
            var existingTrack = _fakers.MusicTrack.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingPlaylist, existingTrack);
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == existingPlaylist.Id);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(existingTrack.Id);
            });
        }

        [Fact]
        public async Task Can_update_ToOne_relationship()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            var existingCompany = _fakers.RecordCompany.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingTrack, existingCompany);
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

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
            });
        }

        [Fact]
        public async Task Can_update_ToMany_relationship()
        {
            // Arrange
            var existingPlaylist = _fakers.Playlist.Generate();
            var existingTrack = _fakers.MusicTrack.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingPlaylist, existingTrack);
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == existingPlaylist.Id);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(existingTrack.Id);
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
                            id = 99999999,
                            attributes = new
                            {
                                name = (string) null
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
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(3);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[0].Detail.Should().Be("The Name field is required.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/name");

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[1].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[1].Detail.Should().Be("The Title field is required.");
            responseDocument.Errors[1].Source.Pointer.Should().Be("/atomic:operations[1]/data/attributes/title");

            responseDocument.Errors[2].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[2].Title.Should().Be("Input validation failed.");
            responseDocument.Errors[2].Detail.Should().Be("The field LengthInSeconds must be between 1 and 1440.");
            responseDocument.Errors[2].Source.Pointer.Should().Be("/atomic:operations[1]/data/attributes/lengthInSeconds");
        }
    }
}
