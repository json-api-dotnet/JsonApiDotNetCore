using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class QueryStringTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

        public QueryStringTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_use_unknown_query_string_parameter()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowUnknownQueryStringParameters = false;

            var route = "/calendars?foo=bar";

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

            var route = "/calendars?foo=bar";

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

            var route = "calendars?" + parameterName + "=";

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
