using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

public sealed class ContentTypeHeaderTests : IClassFixture<IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

    public ContentTypeHeaderTests(IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PoliciesController>();
        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Returns_JsonApi_ContentType_header()
    {
        // Arrange
        const string route = "/policies";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
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

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.AtomicOperations.ToString());
    }

    [Fact]
    public async Task Returns_JsonApi_ContentType_header_with_relaxed_AtomicOperations_extension()
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
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.RelaxedAtomicOperations.ToString()));

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.RelaxedAtomicOperations.ToString());
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

        const string route = "/policies";
        const string contentType = "text/html";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of 'text/html' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }

    [Fact]
    public async Task Denies_unknown_ContentType_header_at_operations_endpoint()
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
        const string contentType = "text/html";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        string detail =
            $"Use '{JsonApiMediaType.AtomicOperations}' or '{JsonApiMediaType.RelaxedAtomicOperations}' instead of 'text/html' for the Content-Type header value.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/policies";
        string contentType = JsonApiMediaType.Default.ToString();

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
    }

    [Fact]
    public async Task Permits_JsonApi_ContentType_header_in_upper_case()
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

        const string route = "/policies";
        string contentType = JsonApiMediaType.Default.ToString().ToUpperInvariant();

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
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

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.AtomicOperations.ToString());
    }

    [Fact]
    public async Task Denies_JsonApi_ContentType_header_with_AtomicOperations_extension_at_operations_endpoint_in_upper_case()
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
        string contentType = JsonApiMediaType.AtomicOperations.ToString().ToUpperInvariant();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        string detail =
            $"Use '{JsonApiMediaType.AtomicOperations}' or '{JsonApiMediaType.RelaxedAtomicOperations}' instead of '{contentType}' for the Content-Type header value.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }

    [Fact]
    public async Task Permits_JsonApi_ContentType_header_with_relaxed_AtomicOperations_extension_at_operations_endpoint()
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
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.RelaxedAtomicOperations.ToString()));

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.RelaxedAtomicOperations.ToString());
    }

    [Fact]
    public async Task Denies_JsonApi_ContentType_header_with_unknown_extension()
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

        const string route = "/policies";
        string contentType = $"{JsonApiMediaType.Default}; ext=something";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/policies";
        string contentType = JsonApiMediaType.AtomicOperations.ToString();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }

    [Fact]
    public async Task Denies_JsonApi_ContentType_header_with_relaxed_AtomicOperations_extension_at_resource_endpoint()
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

        const string route = "/policies";
        string contentType = JsonApiMediaType.RelaxedAtomicOperations.ToString();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/policies";
        string contentType = $"{JsonApiMediaType.Default}; profile=something";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/policies";
        string contentType = $"{JsonApiMediaType.Default}; charset=ISO-8859-4";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/policies";
        string contentType = $"{JsonApiMediaType.Default}; unknown=unexpected";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be($"Use '{JsonApiMediaType.Default}' instead of '{contentType}' for the Content-Type header value.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
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

        const string route = "/operations";
        string contentType = JsonApiMediaType.Default.ToString();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.ShouldHaveCount(1);

        string detail =
            $"Use '{JsonApiMediaType.AtomicOperations}' or '{JsonApiMediaType.RelaxedAtomicOperations}' instead of '{contentType}' for the Content-Type header value.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }
}
