using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.LocalIds
{
    public sealed class AtomicLocalIdTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicLocalIdTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();
        }

        [Fact]
        public async Task Can_create_resource_with_ManyToOne_relationship_using_local_ID()
        {
            // Arrange
            RecordCompany newCompany = _fakers.RecordCompany.Generate();
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("recordCompanies");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newCompany.Name));
                resource.Attributes.ShouldContainKey("countryOfResidence").With(value => value.Should().Be(newCompany.CountryOfResidence));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            short newCompanyId = short.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            Guid newTrackId = Guid.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.ShouldNotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);
                trackInDatabase.OwnedBy.Name.Should().Be(newCompany.Name);
                trackInDatabase.OwnedBy.CountryOfResidence.Should().Be(newCompany.CountryOfResidence);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            Performer newPerformer = _fakers.Performer.Generate();
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newPerformer.ArtistName));
                resource.Attributes.ShouldContainKey("bornAt").With(value => value.As<DateTimeOffset>().Should().BeCloseTo(newPerformer.BornAt));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            int newPerformerId = int.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            Guid newTrackId = Guid.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.ShouldHaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newPerformer.ArtistName);
                trackInDatabase.Performers[0].BornAt.Should().BeCloseTo(newPerformer.BornAt);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newPlaylistName = _fakers.Playlist.Generate().Name;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("playlists");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newPlaylistName));
            });

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            long newPlaylistId = long.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.Tracks.ShouldHaveCount(1);
                playlistInDatabase.Tracks[0].Id.Should().Be(newTrackId);
                playlistInDatabase.Tracks[0].Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Cannot_consume_local_ID_that_is_assigned_in_same_operation()
        {
            // Arrange
            const string companyLocalId = "company-1";

            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                            },
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Local ID cannot be both defined and used within the same operation.");
            error.Detail.Should().Be("Local ID 'company-1' cannot be both defined and used within the same operation.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_reassign_local_ID()
        {
            // Arrange
            string newPlaylistName = _fakers.Playlist.Generate().Name;
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
                            id = Unknown.StringId.For<Lyric, long>()
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Another local ID with the same name is already defined at this point.");
            error.Detail.Should().Be("Another local ID with name 'playlist-1' is already defined at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Can_update_resource_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newTrackGenre = _fakers.MusicTrack.Generate().Genre!;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
                resource.Attributes.ShouldContainKey("genre").With(value => value.Should().BeNull());
            });

            responseDocument.Results[1].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);
                trackInDatabase.Genre.Should().Be(newTrackGenre);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_relationships_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newArtistName = _fakers.Performer.Generate().ArtistName!;
            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(4);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName));
            });

            responseDocument.Results[2].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("recordCompanies");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newCompanyName));
            });

            responseDocument.Results[3].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            int newPerformerId = int.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());
            short newCompanyId = short.Parse(responseDocument.Results[2].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                MusicTrack trackInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .Include(musicTrack => musicTrack.Performers)
                    .FirstWithIdAsync(newTrackId);

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.ShouldNotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);

                trackInDatabase.Performers.ShouldHaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_create_ManyToOne_relationship_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("recordCompanies");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newCompanyName));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            short newCompanyId = short.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.OwnedBy.ShouldNotBeNull();
                trackInDatabase.OwnedBy.Id.Should().Be(newCompanyId);
                trackInDatabase.OwnedBy.Name.Should().Be(newCompanyName);
            });
        }

        [Fact]
        public async Task Can_create_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newArtistName = _fakers.Performer.Generate().ArtistName!;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            int newPerformerId = int.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.ShouldHaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_create_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            string newPlaylistName = _fakers.Playlist.Generate().Name;
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("playlists");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newPlaylistName));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            long newPlaylistId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            Guid newTrackId = Guid.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.Tracks.ShouldHaveCount(1);
                playlistInDatabase.Tracks[0].Id.Should().Be(newTrackId);
                playlistInDatabase.Tracks[0].Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Can_replace_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newArtistName = _fakers.Performer.Generate().ArtistName!;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            int newPerformerId = int.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.ShouldHaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(newPerformerId);
                trackInDatabase.Performers[0].ArtistName.Should().Be(newArtistName);
            });
        }

        [Fact]
        public async Task Can_replace_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            string newPlaylistName = _fakers.Playlist.Generate().Name;
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("playlists");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newPlaylistName));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            long newPlaylistId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            Guid newTrackId = Guid.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.Tracks.ShouldHaveCount(1);
                playlistInDatabase.Tracks[0].Id.Should().Be(newTrackId);
                playlistInDatabase.Tracks[0].Title.Should().Be(newTrackTitle);
            });
        }

        [Fact]
        public async Task Can_add_to_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newArtistName = _fakers.Performer.Generate().ArtistName!;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(3);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            int newPerformerId = int.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.ShouldHaveCount(2);

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
            List<MusicTrack> existingTracks = _fakers.MusicTrack.Generate(2);

            string newPlaylistName = _fakers.Playlist.Generate().Name;
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(4);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("playlists");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newPlaylistName));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[2].Data.Value.Should().BeNull();

            responseDocument.Results[3].Data.Value.Should().BeNull();

            long newPlaylistId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());
            Guid newTrackId = Guid.Parse(responseDocument.Results[1].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(newPlaylistId);

                playlistInDatabase.Name.Should().Be(newPlaylistName);

                playlistInDatabase.Tracks.ShouldHaveCount(3);
                playlistInDatabase.Tracks.Should().ContainSingle(musicTrack => musicTrack.Id == existingTracks[0].Id);
                playlistInDatabase.Tracks.Should().ContainSingle(musicTrack => musicTrack.Id == existingTracks[1].Id);
                playlistInDatabase.Tracks.Should().ContainSingle(musicTrack => musicTrack.Id == newTrackId);
            });
        }

        [Fact]
        public async Task Can_remove_from_OneToMany_relationship_using_local_ID()
        {
            // Arrange
            Performer existingPerformer = _fakers.Performer.Generate();

            string newTrackTitle = _fakers.MusicTrack.Generate().Title;
            string newArtistName1 = _fakers.Performer.Generate().ArtistName!;
            string newArtistName2 = _fakers.Performer.Generate().ArtistName!;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(4);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName1));
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("performers");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("artistName").With(value => value.Should().Be(newArtistName2));
            });

            responseDocument.Results[2].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[3].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[2].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(newTrackId);

                trackInDatabase.Title.Should().Be(newTrackTitle);

                trackInDatabase.Performers.ShouldHaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(existingPerformer.Id);
                trackInDatabase.Performers[0].ArtistName.Should().Be(existingPerformer.ArtistName);
            });
        }

        [Fact]
        public async Task Can_remove_from_ManyToMany_relationship_using_local_ID()
        {
            // Arrange
            Playlist existingPlaylist = _fakers.Playlist.Generate();
            existingPlaylist.Tracks = _fakers.MusicTrack.Generate(2);

            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                                id = existingPlaylist.Tracks[1].StringId
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(4);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.Value.Should().BeNull();

            responseDocument.Results[2].Data.Value.Should().BeNull();

            responseDocument.Results[3].Data.Value.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(existingPlaylist.Id);

                playlistInDatabase.Tracks.ShouldHaveCount(1);
                playlistInDatabase.Tracks[0].Id.Should().Be(existingPlaylist.Tracks[0].Id);
            });
        }

        [Fact]
        public async Task Can_delete_resource_using_local_ID()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                resource.Type.Should().Be("musicTracks");
                resource.Lid.Should().BeNull();
                resource.Attributes.ShouldContainKey("title").With(value => value.Should().Be(newTrackTitle));
            });

            responseDocument.Results[1].Data.Value.Should().BeNull();

            Guid newTrackId = Guid.Parse(responseDocument.Results[0].Data.SingleValue!.Id.ShouldNotBeNull());

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack? trackInDatabase = await dbContext.MusicTracks.FirstWithIdOrDefaultAsync(newTrackId);

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
                            id = Unknown.StringId.For<Lyric, long>()
                        }
                    },
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = Unknown.LocalId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Server-generated value for local ID is not available at this point.");
            error.Detail.Should().Be($"Server-generated value for local ID '{Unknown.LocalId}' is not available at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
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
                            id = Unknown.StringId.For<Lyric, long>()
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "musicTracks",
                            lid = Unknown.LocalId,
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Server-generated value for local ID is not available at this point.");
            error.Detail.Should().Be($"Server-generated value for local ID '{Unknown.LocalId}' is not available at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_data_array()
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
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "lyrics",
                            id = Unknown.StringId.For<Lyric, long>()
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
                                lid = Unknown.LocalId
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Server-generated value for local ID is not available at this point.");
            error.Detail.Should().Be($"Server-generated value for local ID '{Unknown.LocalId}' is not available at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_relationship_data_element()
        {
            // Arrange
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                                        lid = Unknown.LocalId
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Server-generated value for local ID is not available at this point.");
            error.Detail.Should().Be($"Server-generated value for local ID '{Unknown.LocalId}' is not available at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_unassigned_local_ID_in_relationship_data_array()
        {
            // Arrange
            string newPlaylistName = _fakers.Playlist.Generate().Name;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                                            lid = Unknown.LocalId
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Server-generated value for local ID is not available at this point.");
            error.Detail.Should().Be($"Server-generated value for local ID '{Unknown.LocalId}' is not available at this point.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_same_operation()
        {
            // Arrange
            const string trackLocalId = "track-1";
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                            id = Unknown.StringId.For<Lyric, long>()
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'track-1' belongs to resource type 'musicTracks' instead of 'recordCompanies'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[1]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_ref()
        {
            // Arrange
            const string companyLocalId = "company-1";

            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            lid = companyLocalId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'company-1' belongs to resource type 'recordCompanies' instead of 'musicTracks'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
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
                            id = Unknown.StringId.For<Lyric, long>()
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'performer-1' belongs to resource type 'performers' instead of 'playlists'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_data_array()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            const string companyLocalId = "company-1";

            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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
                            id = Unknown.StringId.For<Lyric, long>()
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

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'company-1' belongs to resource type 'recordCompanies' instead of 'performers'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_relationship_data_element()
        {
            // Arrange
            string newPlaylistName = _fakers.Playlist.Generate().Name;
            string newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                                        lid = playlistLocalId
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'playlist-1' belongs to resource type 'playlists' instead of 'recordCompanies'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_of_different_type_in_relationship_data_array()
        {
            // Arrange
            const string performerLocalId = "performer-1";
            string newPlaylistName = _fakers.Playlist.Generate().Name;

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
                            id = Unknown.StringId.For<Lyric, long>()
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
                                            lid = performerLocalId
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Incompatible type in Local ID usage.");
            error.Detail.Should().Be("Local ID 'performer-1' belongs to resource type 'performers' instead of 'musicTracks'.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[2]");
        }
    }
}
