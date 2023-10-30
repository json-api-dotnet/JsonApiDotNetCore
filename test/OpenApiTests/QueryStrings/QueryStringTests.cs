using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.QueryStrings;

public sealed class QueryStringTests : IClassFixture<OpenApiTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> _testContext;

    public QueryStringTests(OpenApiTestContext<OpenApiStartup<QueryStringsDbContext>, QueryStringsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<NodesController>();
        testContext.SwaggerDocumentOutputDirectory = "test/OpenApiEndToEndTests/QueryStrings";
    }

    [Theory]
    [InlineData("/nodes.get")]
    [InlineData("/nodes.head")]
    [InlineData("/nodes.post")]
    [InlineData("/nodes/{id}.get")]
    [InlineData("/nodes/{id}.head")]
    [InlineData("/nodes/{id}.patch")]
    [InlineData("/nodes/{id}/parent.get")]
    [InlineData("/nodes/{id}/parent.head")]
    [InlineData("/nodes/{id}/relationships/parent.get")]
    [InlineData("/nodes/{id}/relationships/parent.head")]
    [InlineData("/nodes/{id}/children.get")]
    [InlineData("/nodes/{id}/children.head")]
    [InlineData("/nodes/{id}/relationships/children.get")]
    [InlineData("/nodes/{id}/relationships/children.head")]
    public async Task Endpoints_have_query_string_parameter(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath($"paths.{endpointPath}").With(verbElement =>
        {
            verbElement.Should().ContainPath("parameters").With(parametersElement =>
            {
                parametersElement.EnumerateArray().Should().ContainSingle(element => element.GetProperty("in").ValueEquals("query")).Subject.With(
                    parameterElement =>
                    {
                        parameterElement.Should().HaveProperty("name", "query");

                        parameterElement.Should().ContainPath("schema").With(schemaElement =>
                        {
                            schemaElement.Should().HaveProperty("type", "object");

                            schemaElement.Should().ContainPath("additionalProperties").With(propertiesElement =>
                            {
                                propertiesElement.Should().HaveProperty("type", "string");
                                propertiesElement.Should().HaveProperty("nullable", true);
                            });

                            schemaElement.Should().HaveProperty("example", null);
                        });
                    });
            });
        });
    }

    [Theory]
    [InlineData("/nodes/{id}.delete")]
    [InlineData("/nodes/{id}/relationships/parent.patch")]
    [InlineData("/nodes/{id}/relationships/children.post")]
    [InlineData("/nodes/{id}/relationships/children.patch")]
    [InlineData("/nodes/{id}/relationships/children.delete")]
    public async Task Endpoints_do_not_have_query_string_parameter(string endpointPath)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        document.Should().ContainPath($"paths.{endpointPath}").With(verbElement =>
        {
            verbElement.Should().ContainPath("parameters").With(parametersElement =>
            {
                parametersElement.EnumerateArray().Should().NotContain(element => element.GetProperty("in").ValueEquals("query"));
            });
        });
    }
}
