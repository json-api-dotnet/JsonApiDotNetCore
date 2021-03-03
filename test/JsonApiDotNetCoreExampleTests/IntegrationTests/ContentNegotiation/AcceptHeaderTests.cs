using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public sealed class AcceptHeaderTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

        public AcceptHeaderTests(ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Permits_no_Accept_headers()
        {
            // Arrange
            const string route = "/policies";

            var acceptHeaders = new MediaTypeWithQualityHeaderValue[0];

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_no_Accept_headers_at_operations_endpoint()
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
                            type = "policies",
                            attributes = new
                            {
                                name = "some"
                            }
                        }
                    }
                }
            };

            const string route = "/operations";
            const string contentType = HeaderConstants.AtomicOperationsMediaType;

            var acceptHeaders = new MediaTypeWithQualityHeaderValue[0];

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_global_wildcard_in_Accept_headers()
        {
            // Arrange
            const string route = "/policies";

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse("*/*")
            };

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_application_wildcard_in_Accept_headers()
        {
            // Arrange
            const string route = "/policies";

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.8"),
                MediaTypeWithQualityHeaderValue.Parse("application/*;q=0.2")
            };

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_JsonApi_without_parameters_in_Accept_headers()
        {
            // Arrange
            const string route = "/policies";

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; q=0.3")
            };

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Permits_JsonApi_with_AtomicOperations_extension_in_Accept_headers_at_operations_endpoint()
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
                            type = "policies",
                            attributes = new
                            {
                                name = "some"
                            }
                        }
                    }
                }
            };

            const string route = "/operations";
            const string contentType = HeaderConstants.AtomicOperationsMediaType;

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + ";ext=\"https://jsonapi.org/ext/atomic\"; q=0.2")
            };

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Denies_JsonApi_with_parameters_in_Accept_headers()
        {
            // Arrange
            const string route = "/policies";

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse("text/html"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; profile=some"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; ext=other"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType + "; unknown=unexpected"),
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.AtomicOperationsMediaType)
            };

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotAcceptable);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
            error.Detail.Should().Be("Please include 'application/vnd.api+json' in the Accept header values.");
        }

        [Fact]
        public async Task Denies_JsonApi_in_Accept_headers_at_operations_endpoint()
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
                            type = "policies",
                            attributes = new
                            {
                                name = "some"
                            }
                        }
                    }
                }
            };

            const string route = "/operations";
            const string contentType = HeaderConstants.AtomicOperationsMediaType;

            MediaTypeWithQualityHeaderValue[] acceptHeaders =
            {
                MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType)
            };

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) =
                await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType, acceptHeaders);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotAcceptable);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
            error.Detail.Should().Be("Please include 'application/vnd.api+json; ext=\"https://jsonapi.org/ext/atomic\"' in the Accept header values.");
        }
    }
}
