using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public sealed class ContentTypeHeaderTests 
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

        public ContentTypeHeaderTests(ExampleIntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services => services.AddControllersFromExampleProject());
        }

        [Fact]
        public async Task Returns_JsonApi_ContentType_header()
        {
            // Arrange
            var route = "/policies";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);
        }

        [Fact]
        public async Task Returns_JsonApi_ContentType_header_with_AtomicOperations_extension()
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

            var route = "/operations";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.AtomicOperationsMediaType);
        }

        [Fact]
        public async Task Denies_unknown_ContentType_header()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = "text/html";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'text/html' for the Content-Type header value.");
        }

        [Fact]
        public async Task Permits_JsonApi_ContentType_header()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.MediaType;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Permits_JsonApi_ContentType_header_with_AtomicOperations_extension_at_operations_endpoint()
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

            var route = "/operations";
            var contentType = HeaderConstants.AtomicOperationsMediaType;

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_with_profile()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.MediaType + "; profile=something";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; profile=something' for the Content-Type header value.");
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_with_extension()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.MediaType + "; ext=something";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; ext=something' for the Content-Type header value.");
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_with_AtomicOperations_extension_at_resource_endpoint()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.AtomicOperationsMediaType;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; ext=\"https://jsonapi.org/ext/atomic\"' for the Content-Type header value.");
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_with_CharSet()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.MediaType + "; charset=ISO-8859-4";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; charset=ISO-8859-4' for the Content-Type header value.");
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_with_unknown_parameter()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "policies",
                    attributes = new
                    {
                        name = "some"
                    }
                }
            };

            var route = "/policies";
            var contentType = HeaderConstants.MediaType + "; unknown=unexpected";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json' instead of 'application/vnd.api+json; unknown=unexpected' for the Content-Type header value.");
        }

        [Fact]
        public async Task Denies_JsonApi_ContentType_header_at_operations_endpoint()
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

            var route = "/operations";
            var contentType = HeaderConstants.MediaType;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody, contentType);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            responseDocument.Errors[0].Title.Should().Be("The specified Content-Type header value is not supported.");
            responseDocument.Errors[0].Detail.Should().Be("Please specify 'application/vnd.api+json; ext=\"https://jsonapi.org/ext/atomic\"' instead of 'application/vnd.api+json' for the Content-Type header value.");
        }
    }
}
