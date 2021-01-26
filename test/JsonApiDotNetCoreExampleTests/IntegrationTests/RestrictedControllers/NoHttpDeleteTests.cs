using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class NoHttpDeleteTests
        : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
        private readonly RestrictionFakers _fakers = new RestrictionFakers();

        public NoHttpDeleteTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_resources()
        {
            // Arrange
            var route = "/sofas";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "sofas",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/sofas";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            var existingSofa = _fakers.Sofa.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Sofas.Add(existingSofa);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "sofas",
                    id = existingSofa.StringId,
                    attributes = new
                    {
                    }
                }
            };

            var route = "/sofas/" + existingSofa.StringId;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Cannot_delete_resource()
        {
            // Arrange
            var existingSofa = _fakers.Sofa.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Sofas.Add(existingSofa);
                await dbContext.SaveChangesAsync();
            });

            var route = "/sofas/" + existingSofa.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.MethodNotAllowed);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
            responseDocument.Errors[0].Title.Should().Be("The request method is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Resource does not support DELETE requests.");
        }
    }
}
