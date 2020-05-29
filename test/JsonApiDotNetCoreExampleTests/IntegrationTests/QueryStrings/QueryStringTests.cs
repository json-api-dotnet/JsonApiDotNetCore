using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class QueryStringTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public QueryStringTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_use_unknown_query_string_parameter()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowUnknownQueryStringParameters = false;

            var route = "/api/v1/articles?foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Unknown query string parameter.");
            responseDocument.Errors[0].Detail.Should().Be("Query string parameter 'foo' is unknown. Set 'AllowUnknownQueryStringParameters' to 'true' in options to ignore unknown parameters.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("foo");
        }

        [Fact]
        public async Task Can_use_unknown_query_string_parameter()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowUnknownQueryStringParameters = true;

            var route = "/api/v1/articles?foo=bar";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("include")]
        [InlineData("filter")]
        [InlineData("sort")]
        [InlineData("page")]
        [InlineData("fields")]
        [InlineData("defaults")]
        [InlineData("nulls")]
        public async Task Cannot_use_empty_query_string_parameter_value(string parameterName)
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowUnknownQueryStringParameters = false;

            var route = "/api/v1/articles?" + parameterName + "=";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Missing query string parameter value.");
            responseDocument.Errors[0].Detail.Should().Be($"Missing value for '{parameterName}' query string parameter.");
            responseDocument.Errors[0].Source.Parameter.Should().Be(parameterName);
        }
    }
}
