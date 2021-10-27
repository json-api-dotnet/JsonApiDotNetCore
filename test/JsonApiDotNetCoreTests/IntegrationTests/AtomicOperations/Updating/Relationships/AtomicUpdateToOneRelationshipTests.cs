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
            Lyric existingLyric = _fakers.Lyric.Generate();
            existingLyric.Track = _fakers.MusicTrack.Generate();

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
                        @ref = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId,
                            relationship = "track"
                        },
                        data = (object)null
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
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Lyric = _fakers.Lyric.Generate();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
                        data = (object)null
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
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "ownedBy"
                        },
                        data = (object)null
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
            Lyric existingLyric = _fakers.Lyric.Generate();
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

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
                        @ref = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId,
                            relationship = "track"
                        },
                        data = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId
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
                Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(existingLyric.Id);

                lyricInDatabase.Track.Id.Should().Be(existingTrack.Id);
            });
        }

        [Fact]
        public async Task Can_create_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            Lyric existingLyric = _fakers.Lyric.Generate();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
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
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);
            });
        }

        [Fact]
        public async Task Can_create_ManyToOne_relationship()
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);
            });
        }

        [Fact]
        public async Task Can_replace_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            Lyric existingLyric = _fakers.Lyric.Generate();
            existingLyric.Track = _fakers.MusicTrack.Generate();

            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

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
                        @ref = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId,
                            relationship = "track"
                        },
                        data = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId
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
                Lyric lyricInDatabase = await dbContext.Lyrics.Include(lyric => lyric.Track).FirstWithIdAsync(existingLyric.Id);

                lyricInDatabase.Track.Id.Should().Be(existingTrack.Id);

                List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().HaveCount(2);
            });
        }

        [Fact]
        public async Task Can_replace_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.Lyric = _fakers.Lyric.Generate();

            Lyric existingLyric = _fakers.Lyric.Generate();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
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
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.Lyric).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.Lyric.Id.Should().Be(existingLyric.Id);

                List<Lyric> lyricsInDatabase = await dbContext.Lyrics.ToListAsync();
                lyricsInDatabase.Should().HaveCount(2);
            });
        }

        [Fact]
        public async Task Can_replace_ManyToOne_relationship()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

            RecordCompany existingCompany = _fakers.RecordCompany.Generate();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                MusicTrack trackInDatabase = await dbContext.MusicTracks.Include(musicTrack => musicTrack.OwnedBy).FirstWithIdAsync(existingTrack.Id);

                trackInDatabase.OwnedBy.Id.Should().Be(existingCompany.Id);

                List<RecordCompany> companiesInDatabase = await dbContext.RecordCompanies.ToListAsync();
                companiesInDatabase.Should().HaveCount(2);
            });
        }

        [Fact]
        public async Task Cannot_create_for_href_element()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new[]
                {
                    new
                    {
                        op = "update",
                        href = "/api/v1/musicTracks/1/relationships/ownedBy"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'href' element is not supported.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/href");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_type_in_ref()
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
                            id = Unknown.StringId.For<Lyric, long>(),
                            relationship = "track"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_type_in_ref()
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
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "ownedBy"
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
            error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
            error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/type");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_ID_in_ref()
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
                            type = "musicTracks",
                            relationship = "ownedBy"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_ID_in_ref()
        {
            // Arrange
            string trackId = Unknown.StringId.For<MusicTrack, Guid>();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = trackId,
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
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
            error.Detail.Should().Be($"Resource of type 'musicTracks' with ID '{trackId}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
            error.Meta.Should().NotContainKey("requestBody");
        }

        [Fact]
        public async Task Cannot_create_for_incompatible_ID_in_ref()
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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = "invalid-guid",
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
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
            error.Title.Should().Be("Failed to deserialize request body: Incompatible 'id' value found.");
            error.Detail.Should().Be("Failed to convert 'invalid-guid' of type 'String' to type 'Guid'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/id");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_ID_and_local_ID_in_ref()
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
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            lid = "local-1",
                            relationship = "ownedBy"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_relationship_in_ref()
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
            error.Title.Should().Be("Failed to deserialize request body: Unknown relationship found.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' does not exist on resource type 'performers'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/relationship");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_data()
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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_array_data()
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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
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
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null' in 'data' element, instead of an array.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_type_in_data()
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
                            type = "lyrics",
                            id = Unknown.StringId.For<Lyric, long>(),
                            relationship = "track"
                        },
                        data = new
                        {
                            id = Unknown.StringId.For<MusicTrack, Guid>()
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
            error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_type_in_data()
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
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.For<Lyric, long>()
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
            error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
            error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data/type");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_missing_ID_in_data()
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
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'id' or 'lid' element is required.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_ID_and_local_ID_in_data()
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
                            type = "musicTracks",
                            id = Unknown.StringId.For<MusicTrack, Guid>(),
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = Unknown.StringId.For<Lyric, long>(),
                            lid = "local-1"
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
            error.Title.Should().Be("Failed to deserialize request body: The 'id' and 'lid' element are mutually exclusive.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_unknown_ID_in_data()
        {
            // Arrange
            MusicTrack existingTrack = _fakers.MusicTrack.Generate();

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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "lyrics",
                            id = lyricId
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
            error.Title.Should().Be("A related resource does not exist.");
            error.Detail.Should().Be($"Related resource of type 'lyrics' with ID '{lyricId}' in relationship 'lyric' does not exist.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");
            error.Meta.Should().NotContainKey("requestBody");
        }

        [Fact]
        public async Task Cannot_create_for_incompatible_ID_in_data()
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
                        @ref = new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId,
                            relationship = "track"
                        },
                        data = new
                        {
                            type = "musicTracks",
                            id = "invalid-guid"
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
            error.Title.Should().Be("Failed to deserialize request body: Incompatible 'id' value found.");
            error.Detail.Should().Be("Failed to convert 'invalid-guid' of type 'String' to type 'Guid'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data/id");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Cannot_create_for_relationship_mismatch_between_ref_and_data()
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
                        @ref = new
                        {
                            type = "musicTracks",
                            id = existingTrack.StringId,
                            relationship = "lyric"
                        },
                        data = new
                        {
                            type = "playlists",
                            id = Unknown.StringId.For<Playlist, long>()
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
            error.Detail.Should().Be("Type 'playlists' is incompatible with type 'lyrics' of relationship 'lyric'.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]/data/type");
            error.Meta["requestBody"].ToString().Should().NotBeNullOrEmpty();
        }
    }
}
