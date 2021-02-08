using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.LocalIds
{
    public sealed class AtomicLocalIdTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicLocalIdTests(ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Can_create_resource_with_ToOne_relationship_using_local_ID()
        {
            // Arrange
            var newCompany = _fakers.RecordCompany.Generate();
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string companyLocalId = "company-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId,
                            attributes = new
                            {
                                name = newCompany.Name,
                                countryOfResidence = newCompany.CountryOfResidence
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
                                title = newTrackTitle
                            },
                            relationships = new
                            {
                                ownedBy = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = companyLocalId
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

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("recordCompanies");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["name"].Should().Be(newCompany.Name);
            responseDocument.Results[0].SingleData.Attributes["countryOfResidence"].Should().Be(newCompany.CountryOfResidence);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            var newCompanyId = short.Parse(responseDocument.Results[0].SingleData.Id);
            var newTrackId = Guid.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);
                trackInDatabase.OwnedBy.Name.Should().Be(newCompany.Name);
                trackInDatabase.OwnedBy.CountryOfResidence.Should().Be(newCompany.CountryOfResidence);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            var newPerformer = _fakers.Performer.Generate();
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId,
                            attributes = new
                            {
                                artistName = newPerformer.ArtistName,
                                bornAt = newPerformer.BornAt
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
                                title = newTrackTitle
                            },
                            relationships = new
                            {
                                performers = new
                                {
                                    data = new[]
                                    {
                                        new
                                        {
                                            type = "performers",
                                            lid = performerLocalId
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

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().Be(newPerformer.ArtistName);
            responseDocument.Results[0].SingleData.Attributes["bornAt"].Should().BeCloseTo(newPerformer.BornAt);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            var newPerformerId = int.Parse(responseDocument.Results[0].SingleData.Id);
            var newTrackId = Guid.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newPerformer.ArtistName);
                trackInDatabase.Performers[0].BornAt.Should().BeCloseTo(newPerformer.BornAt);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newPlaylistName = _fakers.Playlist.Generate().Name;

            const string trackLocalId = "track-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
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
                                            lid = trackLocalId
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

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("playlists");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["name"].Should().Be(newPlaylistName);

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newPlaylistId = long.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(newTrackId);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Cannot_consume_local_ID_that_is_assigned_in_same_operation()
        {
            // Arrange
            const string companyLocalId = "company-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId,
                            relationships = new
                            {
                                parent = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = companyLocalId
                                    }
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Local ID cannot be both defined and used within the same operation.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'company-1' cannot be both defined and used within the same operation.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_reassign_local_ID()
        {
            // Arrange
            var newPlaylistName = _fakers.Playlist.Generate().Name;
            const string playlistLocalId = "playlist-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            attributes = new
                            {
                                name = newPlaylistName
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            attributes = new
                            {
                                name = newPlaylistName
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Another local ID with the same name is already defined at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Another local ID with name 'playlist-1' is already defined at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Can_update_resource_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newTrackGenre = _fakers.MusicTrack.Generate().Genre;

            const string trackLocalId = "track-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
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
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);
            responseDocument.Results[0].SingleData.Attributes["genre"].Should().BeNull();

            responseDocument.Results[1].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);
                trackInDatabase.Genre.Should().Be(newTrackGenre);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_relationships_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newArtistName = _fakers.Performer.Generate().ArtistName;
            var newCompanyName = _fakers.RecordCompany.Generate().Name;

            const string trackLocalId = "track-1";
            const string performerLocalId = "performer-1";
            const string companyLocalId = "company-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId,
                            attributes = new
                            {
                                artistName = newArtistName
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId,
                            attributes = new
                            {
                                name = newCompanyName
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationships = new
                            {
                                ownedBy = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = companyLocalId
                                    }
                                },
                                performers = new
                                {
                                    data = new[]
                                    {
                                        new
                                        {
                                            type = "performers",
                                            lid = performerLocalId
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

            responseDocument.Results.Should().HaveCount(4);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("performers");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["artistName"].Should().Be(newArtistName);

            responseDocument.Results[2].SingleData.Should().NotBeNull();
            responseDocument.Results[2].SingleData.Type.Should().Be("recordCompanies");
            responseDocument.Results[2].SingleData.Lid.Should().BeNull();
            responseDocument.Results[2].SingleData.Attributes["name"].Should().Be(newCompanyName);

            responseDocument.Results[3].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newPerformerId = int.Parse(responseDocument.Results[1].SingleData.Id);
            var newCompanyId = short.Parse(responseDocument.Results[2].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_create_ToOne_relationship_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newCompanyName = _fakers.RecordCompany.Generate().Name;

            const string trackLocalId = "track-1";
            const string companyLocalId = "company-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId,
                            attributes = new
                            {
                                name = newCompanyName
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationship = "ownedBy"
                        },
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("recordCompanies");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["name"].Should().Be(newCompanyName);

            responseDocument.Results[2].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newCompanyId = short.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.Should().NotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);
                trackInDatabase.OwnedBy.Name.Should().Be(newCompanyName);
            });
        }

        [Fact]
        public async Task Can_create_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newArtistName = _fakers.Performer.Generate().ArtistName;

            const string trackLocalId = "track-1";
            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId,
                            attributes = new
                            {
                                artistName = newArtistName
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = performerLocalId
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

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("performers");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["artistName"].Should().Be(newArtistName);

            responseDocument.Results[2].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newPerformerId = int.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_create_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            var newPlaylistName = _fakers.Playlist.Generate().Name;
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string playlistLocalId = "playlist-1";
            const string trackLocalId = "track-1";

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
                            lid = playlistLocalId,
                            attributes = new
                            {
                                name = newPlaylistName
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                lid = trackLocalId
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

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("playlists");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["name"].Should().Be(newPlaylistName);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[2].Data.Should().BeNull();

            var newPlaylistId = long.Parse(responseDocument.Results[0].SingleData.Id);
            var newTrackId = Guid.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(newTrackId);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Can_replace_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();

            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newArtistName = _fakers.Performer.Generate().ArtistName;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Performers.Add(existingPerformer);
                await dbContext.SaveChangesAsync();
            });

            const string trackLocalId = "track-1";
            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            },
                            relationships = new
                            {
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
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId,
                            attributes = new
                            {
                                artistName = newArtistName
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = performerLocalId
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

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("performers");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["artistName"].Should().Be(newArtistName);

            responseDocument.Results[2].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newPerformerId = int.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_replace_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();

            var newPlaylistName = _fakers.Playlist.Generate().Name;
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.MusicTracks.Add(existingTrack);
                await dbContext.SaveChangesAsync();
            });

            const string playlistLocalId = "playlist-1";
            const string trackLocalId = "track-1";

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
                            lid = playlistLocalId,
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
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        @ref = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                lid = trackLocalId
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

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("playlists");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["name"].Should().Be(newPlaylistName);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[2].Data.Should().BeNull();

            var newPlaylistId = long.Parse(responseDocument.Results[0].SingleData.Id);
            var newTrackId = Guid.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(newTrackId);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Can_add_to_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();

            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newArtistName = _fakers.Performer.Generate().ArtistName;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Performers.Add(existingPerformer);
                await dbContext.SaveChangesAsync();
            });

            const string trackLocalId = "track-1";
            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            },
                            relationships = new
                            {
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
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId,
                            attributes = new
                            {
                                artistName = newArtistName
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = performerLocalId
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

            responseDocument.Results.Should().HaveCount(3);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("performers");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["artistName"].Should().Be(newArtistName);

            responseDocument.Results[2].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);
            var newPerformerId = int.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.Should().HaveCount(2);

                trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
                trackInDatabase.Performers[0].ArtistName.Should().Be(existingPerformer.ArtistName);

                trackInDatabase.Performers[1].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[1].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_add_to_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingTracks = _fakers.MusicTrack.Generate(2);

            var newPlaylistName = _fakers.Playlist.Generate().Name;
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string playlistLocalId = "playlist-1";
            const string trackLocalId = "track-1";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.MusicTracks.AddRange(existingTracks);
                await dbContext.SaveChangesAsync();
            });

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
                            lid = playlistLocalId,
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
                                            id = existingTracks[0].StringId
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        @ref = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                lid = trackLocalId
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        @ref = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                id = existingTracks[1].StringId
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

            responseDocument.Results.Should().HaveCount(4);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("playlists");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["name"].Should().Be(newPlaylistName);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[2].Data.Should().BeNull();

            responseDocument.Results[3].Data.Should().BeNull();

            var newPlaylistId = long.Parse(responseDocument.Results[0].SingleData.Id);
            var newTrackId = Guid.Parse(responseDocument.Results[1].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(3);
                playlistInDatabase.PlaylistMusicTracks.Should().ContainSingle(playlistMusicTrack => playlistMusicTrack.MusicTrack.Id == existingTracks[0].Id);
                playlistInDatabase.PlaylistMusicTracks.Should().ContainSingle(playlistMusicTrack => playlistMusicTrack.MusicTrack.Id == existingTracks[1].Id);
                playlistInDatabase.PlaylistMusicTracks.Should().ContainSingle(playlistMusicTrack => playlistMusicTrack.MusicTrack.Id == newTrackId);
            });
        }

        [Fact]
        public async Task Can_remove_from_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingPerformer = _fakers.Performer.Generate();

            var newTrackTitle = _fakers.MusicTrack.Generate().Title;
            var newArtistName1 = _fakers.Performer.Generate().ArtistName;
            var newArtistName2 = _fakers.Performer.Generate().ArtistName;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Performers.Add(existingPerformer);
                await dbContext.SaveChangesAsync();
            });

            const string trackLocalId = "track-1";
            const string performerLocalId1 = "performer-1";
            const string performerLocalId2 = "performer-2";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId1,
                            attributes = new
                            {
                                artistName = newArtistName1
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId2,
                            attributes = new
                            {
                                artistName = newArtistName2
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            },
                            relationships = new
                            {
                                performers = new
                                {
                                    data = new object[]
                                    {
                                        new
                                        {
                                            type = "performers",
                                            id = existingPerformer.StringId
                                        },
                                        new
                                        {
                                            type = "performers",
                                            lid = performerLocalId1
                                        },
                                        new
                                        {
                                            type = "performers",
                                            lid = performerLocalId2
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = performerLocalId1
                            },
                            new
                            {
                                type = "performers",
                                lid = performerLocalId2
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

            responseDocument.Results.Should().HaveCount(4);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().Be(newArtistName1);

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Type.Should().Be("performers");
            responseDocument.Results[1].SingleData.Lid.Should().BeNull();
            responseDocument.Results[1].SingleData.Attributes["artistName"].Should().Be(newArtistName2);

            responseDocument.Results[2].SingleData.Should().NotBeNull();
            responseDocument.Results[2].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[2].SingleData.Lid.Should().BeNull();
            responseDocument.Results[2].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[3].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[2].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
                trackInDatabase.Performers[0].ArtistName.Should().Be(existingPerformer.ArtistName);
            });
        }

        [Fact]
        public async Task Can_remove_from_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            var existingPlaylist = _fakers.Playlist.Generate();
            existingPlaylist.PlaylistMusicTracks = new[]
            {
                new PlaylistMusicTrack
                {
                    MusicTrack = _fakers.MusicTrack.Generate()
                },
                new PlaylistMusicTrack
                {
                    MusicTrack = _fakers.MusicTrack.Generate()
                }
            };

            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string trackLocalId = "track-1";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Playlists.Add(existingPlaylist);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "add",
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
                                lid = trackLocalId
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
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
                                id = existingPlaylist.PlaylistMusicTracks[1].MusicTrack.StringId
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
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
                                lid = trackLocalId
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

            responseDocument.Results.Should().HaveCount(4);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].Data.Should().BeNull();

            responseDocument.Results[2].Data.Should().BeNull();

            responseDocument.Results[3].Data.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var playlistInDatabase = await dbContext.Playlists
                    .Include(playlist => playlist.PlaylistMusicTracks)
                    .ThenInclude(playlistMusicTrack => playlistMusicTrack.MusicTrack)
                    .FirstAsync(playlist => playlist.Id == existingPlaylist.Id);

                playlistInDatabase.PlaylistMusicTracks.Should().HaveCount(1);
                playlistInDatabase.PlaylistMusicTracks[0].MusicTrack.Id.Should().Be(existingPlaylist.PlaylistMusicTracks[0].MusicTrack.Id);
            });
        }

        [Fact]
        public async Task Can_delete_resource_using_local_ID()
        {
            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

            const string trackLocalId = "track-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            attributes = new
                            {
                                title = newTrackTitle
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("musicTracks");
            responseDocument.Results[0].SingleData.Lid.Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["title"].Should().Be(newTrackTitle);

            responseDocument.Results[1].Data.Should().BeNull();

            var newTrackId = Guid.Parse(responseDocument.Results[0].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var trackInDatabase = await dbContext.MusicTracks
                    .FirstOrDefaultAsync(musicTrack => musicTrack.Id == newTrackId);

                trackInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = "doesNotExist"
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'doesNotExist' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_data_element()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            lid = "doesNotExist",
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'doesNotExist' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_data_array()
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
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = "doesNotExist"
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'doesNotExist' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_relationship_data_element()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            relationships = new
                            {
                                ownedBy = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = "doesNotExist"
                                    }
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'doesNotExist' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_relationship_data_array()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "playlists",
                            relationships = new
                            {
                                tracks = new
                                {
                                    data = new[]
                                    {
                                        new
                                        {
                                            type = "musicTracks",
                                            lid = "doesNotExist"
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
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'doesNotExist' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_same_operation()
        {
            // Arrange
            const string trackLocalId = "track-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            lid = trackLocalId,
                            relationships = new
                            {
                                ownedBy = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = trackLocalId
                                    }
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'track-1' belongs to resource type 'musicTracks' instead of 'recordCompanies'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_ref()
        {
            // Arrange
            const string companyLocalId = "company-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = companyLocalId
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'company-1' belongs to resource type 'recordCompanies' instead of 'musicTracks'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_data_element()
        {
            // Arrange
            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "playlists",
                            lid = performerLocalId,
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'performer-1' belongs to resource type 'performers' instead of 'playlists'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_data_array()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();

            const string companyLocalId = "company-1";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.MusicTracks.Add(existingTrack);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            lid = companyLocalId
                        }
                    },
                    new
                    {
                        op = "add",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                lid = companyLocalId
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'company-1' belongs to resource type 'recordCompanies' instead of 'performers'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_relationship_data_element()
        {
            // Arrange
            var newPlaylistName = _fakers.Playlist.Generate().Name;

            const string playlistLocalId = "playlist-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "playlists",
                            lid = playlistLocalId,
                            attributes = new
                            {
                                name = newPlaylistName
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "musicTracks",
                            relationships = new
                            {
                                ownedBy = new
                                {
                                    data = new
                                    {
                                        type = "recordCompanies",
                                        lid = playlistLocalId
                                    }
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'playlist-1' belongs to resource type 'playlists' instead of 'recordCompanies'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_relationship_data_array()
        {
            // Arrange
            const string performerLocalId = "performer-1";

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = 99999999
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "performers",
                            lid = performerLocalId
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "playlists",
                            relationships = new
                            {
                                tracks = new
                                {
                                    data = new[]
                                    {
                                        new
                                        {
                                            type = "musicTracks",
                                            lid = performerLocalId
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
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Type mismatch in local ID usage.");
            responseDocument.Errors[0].Detail.Should().Be("Local ID 'performer-1' belongs to resource type 'performers' instead of 'musicTracks'.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[2]");
        }
    }
}
