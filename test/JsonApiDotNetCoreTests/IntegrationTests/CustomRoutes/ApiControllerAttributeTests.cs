#nullable disable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    public sealed class ApiControllerAttributeTests : IClassFixture<IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;

        public ApiControllerAttributeTests(IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<CiviliansController>();
        }

        [Fact]
        public async Task ApiController_attribute_transforms_NotFound_action_result_without_arguments_into_ProblemDetails()
        {
            // Arrange
            const string route = "/world-civilians/missing";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.Links.About.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        }
    }
}
