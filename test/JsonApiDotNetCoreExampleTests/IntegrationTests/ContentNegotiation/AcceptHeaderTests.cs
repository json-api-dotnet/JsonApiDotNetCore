using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public sealed class AcceptHeaderTests
        : IClassFixture<IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

        public AcceptHeaderTests(IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Permits_no_Accept_headers()
        {
            // Arrange
            var route = "/policies";

            var acceptHeaders = new MediaTypeWithQualityHeaderValue[0];

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_global_wildcard_in_Accept_headers()
        {
            // Arrange
            var route = "/policies";

            var acceptHeaders = new[]
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse("*/*")
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_application_wildcard_in_Accept_headers()
        {
            // Arrange
            var route = "/policies";

            var acceptHeaders = new[]
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.8"),
                MediaTypeWithQualityHeaderValue.Parse("application/*;q=0.2")
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_JsonApi_without_parameters_in_Accept_headers()
        {
            // Arrange
            var route = "/policies";

            var acceptHeaders = new[]
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; q=0.3")
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Denies_JsonApi_with_parameters_in_Accept_headers()
        {
            // Arrange
            var route = "/policies";

            var acceptHeaders = new[]
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected")
            };

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotAcceptable);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            responseDocument.Errors[0].Title.Should().Be("The specified Accept header value does not contain any supported media types.");
            responseDocument.Errors[0].Detail.Should().Be("Please include 'application/vnd.api+json' in the Accept header values.");
        }
    }
}
