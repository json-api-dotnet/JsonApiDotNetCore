using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

public sealed class AcceptHeaderTests : IClassFixture<IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

    public AcceptHeaderTests(IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
        testContext.UseController<PoliciesController>();
    }

    [Fact]
    public async Task Permits_no_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
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

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Permits_global_wildcard_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);
    }

    [Fact]
    public async Task Permits_application_wildcard_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.8"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/*;q=0.2"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);
    }

    [Fact]
    public async Task Permits_JsonApi_without_parameters_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; ext=other"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; q=0.3"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);
    }

    [Fact]
    public async Task Prefers_JsonApi_with_AtomicOperations_extension_in_Accept_headers_at_operations_endpoint()
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
        const string contentType = HeaderConstants.RelaxedAtomicOperationsMediaType;

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType};EXT=atomic-operations; q=0.2"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType};EXT=\"https://jsonapi.org/ext/atomic\"; q=0.8"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.AtomicOperationsMediaType);
    }

    [Fact]
    public async Task Prefers_JsonApi_with_relaxed_AtomicOperations_extension_in_Accept_headers_at_operations_endpoint()
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

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType};EXT=\"https://jsonapi.org/ext/atomic\"; q=0.2"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType};EXT=atomic-operations; q=0.8"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.RelaxedAtomicOperationsMediaType);
    }

    [Fact]
    public async Task Denies_JsonApi_with_parameters_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; ext=other"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{HeaderConstants.MediaType}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.AtomicOperationsMediaType));
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be("Please include 'application/vnd.api+json' in the Accept header values.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Accept");
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

        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(HeaderConstants.MediaType));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.ShouldHaveCount(1);

        const string detail =
            $"Please include '{HeaderConstants.AtomicOperationsMediaType}' or '{HeaderConstants.RelaxedAtomicOperationsMediaType}' in the Accept header values.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Accept");
    }
}
