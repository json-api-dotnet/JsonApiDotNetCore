using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Creating
{
    public sealed class AtomicCreateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicCreateResourceTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
                services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));
            });
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            var newArtistName = _fakers.Performer.Generate().ArtistName;
            var newBornAt = _fakers.Performer.Generate().BornAt;

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
            
            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().Be(newArtistName);
            responseDocument.Results[0].SingleData.Attributes["bornAt"].Should().BeCloseTo(newBornAt);

            var newPerformerId = int.Parse(responseDocument.Results[0].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performerInDatabase = await dbContext.Performers
                    .FirstAsync(performer => performer.Id == newPerformerId);

                performerInDatabase.ArtistName.Should().Be(newArtistName);
                performerInDatabase.BornAt.Should().BeCloseTo(newBornAt);
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
            
            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Type.Should().Be("performers");
            responseDocument.Results[0].SingleData.Attributes["artistName"].Should().BeNull();
            responseDocument.Results[0].SingleData.Attributes["bornAt"].Should().BeCloseTo(default(DateTimeOffset));

            var newPerformerId = int.Parse(responseDocument.Results[0].SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performerInDatabase = await dbContext.Performers
                    .FirstAsync(performer => performer.Id == newPerformerId);

                performerInDatabase.ArtistName.Should().BeNull();
                performerInDatabase.BornAt.Should().Be(default);
            });
        }

        [Fact]
        public async Task Can_create_resources()
        {
            // Arrange
            const int elementCount = 5;

            var newTracks = _fakers.MusicTrack.Generate(elementCount);

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
            
            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                responseDocument.Results[index].SingleData.Should().NotBeNull();
                responseDocument.Results[index].SingleData.Type.Should().Be("musicTracks");
                responseDocument.Results[index].SingleData.Attributes["title"].Should().Be(newTracks[index].Title);
                responseDocument.Results[index].SingleData.Attributes["lengthInSeconds"].Should().BeApproximately(newTracks[index].LengthInSeconds, 0.00000000001M);
                responseDocument.Results[index].SingleData.Attributes["genre"].Should().Be(newTracks[index].Genre);
                responseDocument.Results[index].SingleData.Attributes["releasedAt"].Should().BeCloseTo(newTracks[index].ReleasedAt);
            }

            var newTrackIds = responseDocument.Results.Select(result => Guid.Parse(result.SingleData.Id));

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var tracksInDatabase = await dbContext.MusicTracks
                    .Where(musicTrack => newTrackIds.Contains(musicTrack.Id))
                    .ToListAsync();

                tracksInDatabase.Should().HaveCount(elementCount);

                for (int index = 0; index < elementCount; index++)
                {
                    var trackInDatabase = tracksInDatabase.Single(musicTrack => musicTrack.Id == Guid.Parse(responseDocument.Results[index].SingleData.Id));

                    trackInDatabase.Title.Should().Be(newTracks[index].Title);
                    trackInDatabase.LengthInSeconds.Should().BeApproximately(newTracks[index].LengthInSeconds, 0.00000000001M);
                    trackInDatabase.Genre.Should().Be(newTracks[index].Genre);
                    trackInDatabase.ReleasedAt.Should().BeCloseTo(newTracks[index].ReleasedAt);
                }
            });
        }
    }
}
