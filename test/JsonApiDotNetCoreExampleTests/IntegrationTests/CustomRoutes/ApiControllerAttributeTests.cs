using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    public sealed class ApiControllerAttributeTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;

        public ApiControllerAttributeTests(ExampleIntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task ApiController_attribute_transforms_NotFound_action_result_without_arguments_into_ProblemDetails()
        {
            // Arrange
            var route = "/world-civilians/missing";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].Links.About.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }
    }
}
