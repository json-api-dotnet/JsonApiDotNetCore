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

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Updating.Relationships
{
    public sealed class AtomicRemoveFromToManyRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicRemoveFromToManyRelationshipTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();
        }

        [Fact]
        public async Task Cannot_remove_from_ManyToOne_relationship()
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
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "ownedBy"
                        },
                        data = new
                        {
                            type = "recordCompanies",
                            id = existingTrack.OwnedBy.StringId
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Only to-many relationships can be targeted through this operation.");
            error.Detail.Should().Be("Relationship 'ownedBy' must be a to-many relationship.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/relationship");
        }

        [Fact]
        public async Task Can_remove_from_OneToMany_relationship()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Performers = _fakers.Performer.Generate(3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Performer>();
                dbContext.MusicTracks.Add(existingTrack);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
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
                                id = existingTrack.Performers[0].StringId
                            }
                        }
                    },
                    new
                    {
                        op = "remove",
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
                                id = existingTrack.Performers[2].StringId
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(existingTrack.Performers[1].Id);

                List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().HaveCount(3);
            });
        }

        [Fact]
        public async Task Can_remove_from_ManyToMany_relationship()
        {
            // Arrange
            Playlist existingPlaylist = _fakers.Playlist.Generate();
            existingPlaylist.Tracks = _fakers.MusicTrack.Generate(3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<MusicTrack>();
                dbContext.Playlists.Add(existingPlaylist);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
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
                                id = existingPlaylist.Tracks[0].StringId
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
                                id = existingPlaylist.Tracks[2].StringId
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Playlist playlistInDatabase = await dbContext.Playlists.Include(playlist => playlist.Tracks).FirstWithIdAsync(existingPlaylist.Id);

                playlistInDatabase.Tracks.Should().HaveCount(1);
                playlistInDatabase.Tracks[0].Id.Should().Be(existingPlaylist.Tracks[1].Id);

                List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();

                tracksInDatabase.Should().HaveCount(3);
            });
        }

        [Fact]
        public async Task Cannot_remove_for_href_element()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        href = "/api/v1/musicTracks/1/relationships/performers"
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Usage of the 'href' element is not supported.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/href");
        }

        [Fact]
        public async Task Cannot_remove_for_missing_type_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            id = Unknown.StringId.For<Playlist, long>(),
                            relationship = "tracks"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            error.Detail.Should().Be("Expected 'type' element in 'ref' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        }

        [Fact]
        public async Task Cannot_remove_for_unknown_type_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.For<Playlist, long>(),
                            relationship = "tracks"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/type");
        }

        [Fact]
        public async Task Cannot_remove_for_missing_ID_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            relationship = "performers"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'id' or 'lid' element.");
            error.Detail.Should().Be("Expected 'id' or 'lid' element in 'ref' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        }

        [Fact]
        public async Task Cannot_remove_for_unknown_ID_in_ref()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.MusicTracks.Add(existingTrack);
                await dbContext.SaveChangesAsync();
            });

            string companyId = Unknown.StringId.For<RecordCompany, short>();

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "recordCompanies",
                            id = companyId,
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
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("The requested resource does not exist.");
            error.Detail.Should().Be($"Resource of type 'recordCompanies' with ID '{companyId}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_remove_for_ID_and_local_ID_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            lid = "local-1",
                            relationship = "performers"
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'id' or 'lid' element.");
            error.Detail.Should().Be("Expected 'id' or 'lid' element in 'ref' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
        }

        [Fact]
        public async Task Cannot_remove_for_unknown_relationship_in_ref()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "performers",
                            id = Unknown.StringId.For<Performer, int>(),
                            relationship = Unknown.Relationship
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body includes unknown relationship.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/relationship");
        }

        [Fact]
        public async Task Cannot_remove_for_null_data()
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
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "performers"
                        },
                        data = (object)null
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Expected data[] element for to-many relationship.");
            error.Detail.Should().Be("Expected data[] element for 'performers' relationship.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
        }

        [Fact]
        public async Task Cannot_remove_for_missing_type_in_data()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "playlists",
                            id = Unknown.StringId.For<Playlist, long>(),
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                id = Unknown.StringId.For<MusicTrack, Guid>()
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            error.Detail.Should().Be("Expected 'type' element in 'data' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data[0]");
        }

        [Fact]
        public async Task Cannot_remove_for_unknown_type_in_data()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = Unknown.ResourceType,
                                id = Unknown.StringId.For<Performer, int>()
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data[0]/type");
        }

        [Fact]
        public async Task Cannot_remove_for_missing_ID_in_data()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers"
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'id' or 'lid' element.");
            error.Detail.Should().Be("Expected 'id' or 'lid' element in 'data' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data[0]");
        }

        [Fact]
        public async Task Cannot_remove_for_ID_and_local_ID_in_data()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "performers"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "performers",
                                id = Unknown.StringId.For<Performer, int>(),
                                lid = "local-1"
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Request body must include 'id' or 'lid' element.");
            error.Detail.Should().Be("Expected 'id' or 'lid' element in 'data' element.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data[0]");
        }

        [Fact]
        public async Task Cannot_remove_for_unknown_IDs_in_data()
        {
            // Arrange
            RecordCompany existingCompany = _fakers.RecordCompany.Generate();

            string[] trackIds =
            {
                Unknown.StringId.For<MusicTrack, Guid>(),
                Unknown.StringId.AltFor<MusicTrack, Guid>()
            };

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
                        op = "remove",
                        @ref = new
                        {
                            type = "recordCompanies",
                            id = existingCompany.StringId,
                            relationship = "tracks"
                        },
                        data = new[]
                        {
                            new
                            {
                                type = "musicTracks",
                                id = trackIds[0]
                            },
                            new
                            {
                                type = "musicTracks",
                                id = trackIds[1]
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(2);

            ErrorObject error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error1.Title.Should().Be("A related resource does not exist.");
            error1.Detail.Should().Be($"Related resource of type 'musicTracks' with ID '{trackIds[0]}' in relationship 'tracks' does not exist.");
            error1.Source.Pointer.Should().Be("/atomic:operations[0]");

            ErrorObject error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error2.Title.Should().Be("A related resource does not exist.");
            error2.Detail.Should().Be($"Related resource of type 'musicTracks' with ID '{trackIds[1]}' in relationship 'tracks' does not exist.");
            error2.Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_remove_for_relationship_mismatch_between_ref_and_data()
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
                        op = "remove",
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
                                type = "playlists",
                                id = Unknown.StringId.For<Playlist, long>()
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Relationship contains incompatible resource type.");
            error.Detail.Should().Be("Relationship 'performers' contains incompatible resource type 'playlists'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data[0]/type");
        }

        [Fact]
        public async Task Can_remove_with_empty_data_array()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Performers = _fakers.Performer.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Performer>();
                dbContext.MusicTracks.Add(existingTrack);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "remove",
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "performers"
                        },
                        data = Array.Empty<object>()
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAtomicAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Performers).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Performers.Should().HaveCount(1);
                trackInDatabase.Performers[0].Id.Should().Be(existingTrack.Performers[0].Id);
            });
        }
    }
}
