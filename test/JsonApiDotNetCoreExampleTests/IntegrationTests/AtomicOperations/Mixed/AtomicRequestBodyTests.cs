using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class AtomicRequestBodyTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
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
            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, null);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Missing request body.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                var tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Cannot_process_for_broken_JSON_request_body()
        {
            // Arrange
            var requestBody = "{\"atomic__operations\":[{\"op\":";

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Unexpected end of content while loading JObject.");
            responseDocument.Errors[0].Source.Pointer.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_process_empty_operations_array()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[0]
                {
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: No operations found.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                var tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
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

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Error converting value \"merge\" to type");
            responseDocument.Errors[0].Source.Pointer.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var performersInDatabase = await dbContext.Performers.ToListAsync();
                performersInDatabase.Should().BeEmpty();

                var tracksInDatabase = await dbContext.MusicTracks.ToListAsync();
                tracksInDatabase.Should().BeEmpty();
            });
        }
    }
}
