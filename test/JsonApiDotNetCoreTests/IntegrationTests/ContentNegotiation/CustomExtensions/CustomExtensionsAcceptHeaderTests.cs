using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

public sealed class CustomExtensionsAcceptHeaderTests : IClassFixture<IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> _testContext;

    public CustomExtensionsAcceptHeaderTests(IntegrationTestContext<TestableStartup<PolicyDbContext>, PolicyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
        testContext.UseController<PoliciesController>();

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiContentNegotiator, ServerTimeContentNegotiator>();
            services.AddScoped<IResponseMeta, ServerTimeResponseMeta>();
            services.AddScoped<RequestDocumentStore>();
        });

        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeExtensions(ServerTimeMediaTypeExtension.ServerTime, ServerTimeMediaTypeExtension.RelaxedServerTime);
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
    public async Task Prefers_first_match_from_GetPossibleMediaTypes_with_largest_number_of_extensions()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.Default.ToString()));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.RelaxedServerTime.ToString()));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ServerTimeMediaTypes.ServerTime.ToString()));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(ServerTimeMediaTypes.ServerTime.ToString());
    }

    [Fact]
    public async Task Prefers_quality_factor_over_largest_number_of_extensions()
    {
        // Arrange
        const string route = "/policies";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{ServerTimeMediaTypes.ServerTime}; q=0.2"));
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse($"{JsonApiMediaType.Default}; q=0.8"));
        };

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.Default.ToString());
    }

    [Fact]
    public async Task Denies_extensions_mismatch_between_ContentType_and_Accept_header()
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
            headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.AtomicOperations.ToString()));

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, contentType, setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotAcceptable);

        responseDocument.Errors.Should().HaveCount(1);

        string detail = $"Include '{JsonApiMediaType.AtomicOperations}' or '{ServerTimeMediaTypes.AtomicOperationsWithServerTime}' or " +
            $"'{JsonApiMediaType.RelaxedAtomicOperations}' or '{ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime}' in the Accept header values.";

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
        error.Title.Should().Be("The specified Accept header value does not contain any supported media types.");
        error.Detail.Should().Be(detail);
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be("Accept");
    }
}
