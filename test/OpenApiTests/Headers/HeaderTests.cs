using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.Headers;

public sealed class HeaderTests : IClassFixture<OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> _testContext;

    public HeaderTests(OpenApiTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CountriesController>();

        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/Headers";
    }

    [Theory]
    [InlineData("/countries.get")]
    [InlineData("/countries.head")]
    [InlineData("/countries/{id}.get")]
    [InlineData("/countries/{id}.head")]
    [InlineData("/countries/{id}/languages.get")]
    [InlineData("/countries/{id}/languages.head")]
    [InlineData("/countries/{id}/relationships/languages.get")]
    [InlineData("/countries/{id}/relationships/languages.head")]
    public async Task Get_and_head_endpoints_have_caching_headers(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"paths.{endpointPath}.parameters").With(parametersElement =>
        {
            parametersElement.EnumerateArray().Should().ContainSingle(parameterElement => parameterElement.GetProperty("in").ValueEquals("header")).Subject
                .With(parameterElement =>
                {
                    parameterElement.Should().HaveProperty("name", "If-None-Match");

                    parameterElement.Should().HaveProperty("description",
                        "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.");

                    parameterElement.Should().ContainPath("schema").With(schemaElement =>
                    {
                        schemaElement.Should().HaveProperty("type", "string");
                        schemaElement.Should().HaveProperty("nullable", true);
                    });
                });
        });

        document.Should().ContainPath($"paths.{endpointPath}.responses.200.headers.ETag").With(AssertEtag);
        document.Should().ContainPath($"paths.{endpointPath}.responses.304.headers.ETag").With(AssertEtag);

        return;

        static void AssertEtag(JsonElement etagElement)
        {
            etagElement.Should().HaveProperty("description",
                "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.");

            etagElement.Should().HaveProperty("required", true);

            etagElement.Should().ContainPath("schema").With(schemaElement =>
            {
                schemaElement.Should().HaveProperty("type", "string");
            });
        }
    }

    [Theory]
    [InlineData("/countries.post")]
    [InlineData("/countries/{id}.patch")]
    [InlineData("/countries/{id}.delete")]
    [InlineData("/countries/{id}/relationships/languages.post")]
    [InlineData("/countries/{id}/relationships/languages.patch")]
    [InlineData("/countries/{id}/relationships/languages.delete")]
    public async Task Post_patch_and_delete_endpoints_do_not_have_caching_headers(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"paths.{endpointPath}.responses").With(responsesElement =>
        {
            foreach (JsonProperty responseProperty in responsesElement.EnumerateObject())
            {
                responseProperty.Value.Should().NotContainPath("headers.ETag");
                responseProperty.Value.Should().NotContainPath("headers.Content-Length");
            }
        });

        document.Should().ContainPath($"paths.{endpointPath}.parameters").With(parametersElement =>
        {
            parametersElement.EnumerateArray().Should().NotContain(parameterElement => parameterElement.GetProperty("name").ValueEquals("If-None-Match"));
        });
    }

    [Theory]
    [InlineData("/countries.head")]
    [InlineData("/countries/{id}.head")]
    [InlineData("/countries/{id}/languages.head")]
    [InlineData("/countries/{id}/relationships/languages.head")]
    public async Task Head_endpoints_have_content_length_response_header(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"paths.{endpointPath}.responses.200.headers.Content-Length").With(contentLengthElement =>
        {
            contentLengthElement.Should().HaveProperty("description", "Size of the HTTP response body, in bytes.");

            contentLengthElement.Should().HaveProperty("required", true);

            contentLengthElement.Should().ContainPath("schema").With(schemaElement =>
            {
                schemaElement.Should().HaveProperty("type", "integer");
            });
        });
    }

    [Theory]
    [InlineData("/countries.post")]
    public async Task Post_endpoints_have_location_response_header(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath($"paths.{endpointPath}.responses.201.headers.Location").With(locationElement =>
        {
            locationElement.Should().HaveProperty("description", "The URL at which the newly created JSON:API resource can be retrieved.");

            locationElement.Should().HaveProperty("required", true);

            locationElement.Should().ContainPath("schema").With(schemaElement =>
            {
                schemaElement.Should().HaveProperty("type", "string");
                schemaElement.Should().HaveProperty("format", "uri");
            });
        });
    }
}
