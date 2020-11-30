using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class LocalIdTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public LocalIdTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
                services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));
            });
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

            var route = "/api/v1/operations";

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
        public async Task Cannot_consume_unassigned_local_ID()
        {
            // TODO: @OPS: This can occur at multiple places: in a 'ref', in a to-one relationships, in an element of a to-many relationship etc...

            // Arrange
            var newTrackTitle = _fakers.MusicTrack.Generate().Title;

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
                                        lid = "company-1"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'company-1' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
        }

        [Fact]
        public async Task Cannot_consume_local_ID_that_is_assigned_in_same_operation()
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

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Server-generated value for local ID is not available at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Server-generated value for local ID 'track-1' is not available at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[0]");
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

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Another local ID with the same name is already in use at this point.");
            responseDocument.Errors[0].Detail.Should().Be("Another local ID with name 'playlist-1' is already in use at this point.");
            responseDocument.Errors[0].Source.Pointer.Should().Be("/atomic:operations[1]");
        }
    }
}
