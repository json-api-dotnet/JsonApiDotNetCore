using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    public sealed class NoHttpPostTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
        private readonly RestrictionFakers _fakers = new();

        public NoHttpPostTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<BlockingHttpPostController>();
        }

        [Fact]
        public async Task Can_get_resources()
        {
            // Arrange
            const string route = "/tables";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

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

            const string route = "/tables";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.MethodNotAllowed);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            error.Title.Should().Be("The request method is not allowed.");
            error.Detail.Should().Be("Endpoint does not support POST requests.");
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            Table existingTable = _fakers.Table.Generate();

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

            string route = $"/tables/{existingTable.StringId}";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Can_delete_resource()
        {
            // Arrange
            Table existingTable = _fakers.Table.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Tables.Add(existingTable);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/tables/{existingTable.StringId}";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }
    }
}
