using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class NoHttpPostTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
        private readonly RestrictionFakers _fakers = new RestrictionFakers();

        public NoHttpPostTests(ExampleIntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_resources()
        {
            // Arrange
            var route = "/tables";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_create_resource()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "tables",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/tables";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.MethodNotAllowed);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            responseDocument.Errors[0].Title.Should().Be("The request method is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Resource does not support POST requests.");
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            var existingTable = _fakers.Table.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Tables.Add(existingTable);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "tables",
                    id = existingTable.StringId,
                    attributes = new
                    {
                    }
                }
            };

            var route = "/tables/" + existingTable.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Can_delete_resource()
        {
            // Arrange
            var existingTable = _fakers.Table.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Tables.Add(existingTable);
                await dbContext.SaveChangesAsync();
            });

            var route = "/tables/" + existingTable.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }
    }
}
