using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class AtomicRequestBodyTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;

        public AtomicRequestBodyTests(ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Cannot_process_for_missing_request_body()
        {
            // Arrange
            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, null);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Missing request body.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Cannot_process_for_broken_JSON_request_body()
        {
            // Arrange
            const string requestBody = "{\"atomic__operations\":[{\"op\":";

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body.");
            error.Detail.Should().StartWith("Unexpected end of content while loading JObject.");
            error.Source.Pointer.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_process_empty_operations_array()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[0]
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body: No operations found.");
            error.Detail.Should().BeNull();
            error.Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Cannot_process_for_unknown_operation_code()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "merge",
                        data = new
                        {
                            type = "performers",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/operations";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body.");
            error.Detail.Should().StartWith("Error converting value \"merge\" to type");
            error.Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                List<MusicTrack> tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().BeEmpty();
            });
        }
    }
}
