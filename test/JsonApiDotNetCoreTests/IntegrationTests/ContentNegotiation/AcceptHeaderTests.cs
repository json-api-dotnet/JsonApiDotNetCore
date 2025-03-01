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
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
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
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
    }

    [Fact]
    public async Task Permits_JsonApi_without_parameters_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; ext=other"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; q=0.3"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
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
        string contentType = JsonApiMediaType.AtomicOperations.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.Default.ToString()));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default};EXT=atomic; q=0.8"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default};EXT=\"https://jsonapi.org/ext/atomic\"; q=0.2"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.AtomicOperations.ToString());
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
        string contentType = JsonApiMediaType.RelaxedAtomicOperations.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.Default.ToString()));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default};EXT=\"https://jsonapi.org/ext/atomic\"; q=0.8"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default};EXT=atomic; q=0.2"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.RelaxedAtomicOperations.ToString());
    }

    [Fact]
    public async Task Denies_JsonApi_with_parameters_in_Accept_headers()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; profile=some"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; ext=other"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; unknown=unexpected"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.AtomicOperations.ToString()));
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be($"Include '{JsonApiMediaType.Default}' in the Accept header values.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Accept");
    }

    [Fact]
    public async Task Denies_no_Accept_headers_at_operations_endpoint()
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
        string contentType = JsonApiMediaType.AtomicOperations.ToString();

        Action<HttpRequestHeaders> requestHeaders = _ =>
        {
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, requestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be($"Include '{JsonApiMediaType.AtomicOperations}' or '{JsonApiMediaType.RelaxedAtomicOperations}' in the Accept header values.");
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
        string contentType = JsonApiMediaType.AtomicOperations.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.Default.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be($"Include '{JsonApiMediaType.AtomicOperations}' or '{JsonApiMediaType.RelaxedAtomicOperations}' in the Accept header values.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Accept");
    }
}
