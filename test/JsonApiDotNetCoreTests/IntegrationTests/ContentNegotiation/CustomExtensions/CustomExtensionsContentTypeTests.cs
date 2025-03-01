using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

public sealed class CustomExtensionsContentTypeTests : IClassFixture<IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
{
    private static readonly DateTimeOffset CurrentTime = 31.December(2024).At(21, 53, 40).AsUtc();
    private readonly IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

    public CustomExtensionsContentTypeTests(IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PoliciesController>();
        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiContentNegotiator, ServerTimeContentNegotiator>();
            services.AddScoped<IResponseMeta, ServerTimeResponseMeta>();

            services.AddScoped<DocumentAdapter>();
            services.AddScoped<RequestDocumentStore>();

            services.AddScoped<IDocumentAdapter>(serviceProvider =>
            {
                var documentAdapter = serviceProvider.GetRequiredService<DocumentAdapter>();
                var requestDocumentStore = serviceProvider.GetRequiredService<RequestDocumentStore>();
                return new CapturingDocumentAdapter(documentAdapter, requestDocumentStore);
            });
        });

        testContext.PostConfigureServices(services => services.Replace(
            ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(CurrentTime, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")))));

        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeExtensions(ServerTimeMediaTypeExtension.ServerTime, ServerTimeMediaTypeExtension.RelaxedServerTime);
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
    public async Task Permits_JsonApi_ContentType_header_with_ServerTime_extension()
    {
        // Arrange
        var requestBody = new
        {
            meta = new
            {
                useLocalTime = true
            },
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
        string contentType = ServerTimeMediaTypes.ServerTime.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.ServerTime.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(ServerTimeMediaTypes.ServerTime.ToString());

        responseDocument.Meta.ShouldContainKey("localServerTime").With(time =>
            time.ShouldNotBeNull().ToString().Should().Be("2025-01-01T06:53:40.0000000+09:00"));
    }

    [Fact]
    public async Task Permits_JsonApi_ContentType_header_with_relaxed_ServerTime_extension()
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
        string contentType = ServerTimeMediaTypes.RelaxedServerTime.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.RelaxedServerTime.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(ServerTimeMediaTypes.RelaxedServerTime.ToString());

        responseDocument.Meta.ShouldContainKey("utcServerTime").With(time => time.ShouldNotBeNull().ToString().Should().Be("2024-12-31T21:53:40.0000000Z"));
    }

    [Fact]
    public async Task Permits_JsonApi_ContentType_header_with_AtomicOperations_and_ServerTime_extension_at_operations_endpoint()
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
        string contentType = ServerTimeMediaTypes.AtomicOperationsWithServerTime.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.AtomicOperationsWithServerTime.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(ServerTimeMediaTypes.AtomicOperationsWithServerTime.ToString());

        responseDocument.Meta.ShouldContainKey("utcServerTime").With(time => time.ShouldNotBeNull().ToString().Should().Be("2024-12-31T21:53:40.0000000Z"));
    }

    [Fact]
    public async Task Permits_JsonApi_ContentType_header_with_relaxed_AtomicOperations_and_relaxed_ServerTime_extension_at_operations_endpoint()
    {
        // Arrange
        var requestBody = new
        {
            meta = new
            {
                useLocalTime = true
            },

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
        string contentType = ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime.ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime.ToString());

        responseDocument.Meta.ShouldContainKey("localServerTime");
    }

    [Fact]
    public async Task Denies_JsonApi_ContentType_header_with_AtomicOperations_extension_and_ServerTime_at_resource_endpoint()
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
        string contentType = ServerTimeMediaTypes.AtomicOperationsWithServerTime.ToString();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.Should().HaveCount(1);

        string detail = $"Use '{JsonApiMediaType.Default}' or '{ServerTimeMediaTypes.ServerTime}' or " +
            $"'{ServerTimeMediaTypes.RelaxedServerTime}' instead of '{contentType}' for the Content-Type header value.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }

    [Fact]
    public async Task Denies_JsonApi_ContentType_header_with_relaxed_ServerTime_at_operations_endpoint()
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
        string contentType = ServerTimeMediaTypes.RelaxedServerTime.ToString();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnsupportedMediaType);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());

        responseDocument.Errors.Should().HaveCount(1);

        string detail = $"Use '{JsonApiMediaType.AtomicOperations}' or '{ServerTimeMediaTypes.AtomicOperationsWithServerTime}' or " +
            $"'{JsonApiMediaType.RelaxedAtomicOperations}' or '{ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime}' " +
            $"instead of '{contentType}' for the Content-Type header value.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        error.Title.Should().Be("The specified Content-Type header value is not supported.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Content-Type");
    }
}
