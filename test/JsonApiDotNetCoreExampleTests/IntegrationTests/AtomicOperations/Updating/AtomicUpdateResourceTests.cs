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

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Updating
{
    public sealed class AtomicUpdateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicUpdateResourceTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
                services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));
            });
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            var existingTrack = _fakers.MusicTrack.Generate();
            existingTrack.OwnedBy = _fakers.RecordCompany.Generate();

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
                                title = newTitle
                            }
                        }
                    }
                }
            };

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var tracksInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                tracksInDatabase.Title.Should().Be(newTitle);
                tracksInDatabase.Genre.Should().Be(existingTrack.Genre);

                tracksInDatabase.OwnedBy.Should().NotBeNull();
                tracksInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
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

            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(1);
            responseDocument.Results[0].SingleData.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var tracksInDatabase = await dbContext.MusicTracks
                    .Include(musicTrack => musicTrack.OwnedBy)
                    .FirstAsync(musicTrack => musicTrack.Id == existingTrack.Id);

                tracksInDatabase.Title.Should().Be(existingTrack.Title);
                tracksInDatabase.Genre.Should().Be(existingTrack.Genre);

                tracksInDatabase.OwnedBy.Should().NotBeNull();
                tracksInDatabase.OwnedBy.Id.Should().Be(existingTrack.OwnedBy.Id);
            });
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
            
            var route = "/api/v1/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(elementCount);

            for (int index = 0; index < elementCount; index++)
            {
                responseDocument.Results[index].SingleData.Should().BeNull();
            }

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
    }
}
