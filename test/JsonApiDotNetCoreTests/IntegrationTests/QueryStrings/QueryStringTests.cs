using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed class QueryStringTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

    public QueryStringTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CalendarsController>();

        var options = (JsonApiOptions)_testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownQueryStringParameters = false;
    }

    [Fact]
    public async Task Cannot_use_unknown_query_string_parameter()
    {
        // Arrange
        const string route = "/calendars?foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Unknown query string parameter.");

        error.Detail.Should().Be("Query string parameter 'foo' is unknown. " +
            "Set 'AllowUnknownQueryStringParameters' to 'true' in options to ignore unknown parameters.");

        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("foo");
    }

    [Fact]
    public async Task Can_use_unknown_query_string_parameter()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownQueryStringParameters = true;

        const string route = "/calendars?foo=bar";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bar")]
    public async Task Can_use_empty_query_string_parameter_name(string parameterValue)
    {
        // Arrange
        string route = $"calendars?={parameterValue}";

        // Act
        (HttpResponseMessage httpResponse, Document _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("filter")]
    [InlineData("sort")]
    [InlineData("page[size]")]
    [InlineData("page[number]")]
    public async Task Cannot_use_empty_query_string_parameter_value(string parameterName)
    {
        // Arrange
        string route = $"calendars?{parameterName}=";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Missing query string parameter value.");
        error.Detail.Should().Be($"Missing value for '{parameterName}' query string parameter.");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName);
    }

    [Fact]
    public async Task Aggregates_multiple_query_string_errors()
    {
        // Arrange
        const string route = "calendars?include=bad&filter=equals(missing,'1')&fields[wrong]=id&other=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Included.Should().BeNull();
        responseDocument.Errors.Should().HaveCount(4);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error1.Title.Should().Be("The specified include is invalid.");
        error1.Detail.Should().StartWith("Relationship 'bad' does not exist on resource type 'calendars'. Failed at position ");
        error1.Source.Should().NotBeNull();
        error1.Source.Parameter.Should().Be("include");
        error1.Meta.Should().HaveInStackTrace($"*{typeof(IncludeQueryStringParameterReader).FullName}*");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error2.Title.Should().Be("The specified filter is invalid.");
        error2.Detail.Should().StartWith("Field 'missing' does not exist on resource type 'calendars'. Failed at position ");
        error2.Source.Should().NotBeNull();
        error2.Source.Parameter.Should().Be("filter");
        error2.Meta.Should().HaveInStackTrace($"*{typeof(FilterQueryStringParameterReader).FullName}*");

        ErrorObject error3 = responseDocument.Errors[2];
        error3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error3.Title.Should().Be("The specified fieldset is invalid.");
        error3.Detail.Should().StartWith("Resource type 'wrong' does not exist. Failed at position ");
        error3.Source.Should().NotBeNull();
        error3.Source.Parameter.Should().Be("fields[wrong]");
        error3.Meta.Should().HaveInStackTrace($"*{typeof(SparseFieldSetQueryStringParameterReader).FullName}*");

        ErrorObject error4 = responseDocument.Errors[3];
        error4.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error4.Title.Should().Be("Unknown query string parameter.");
        error4.Detail.Should().StartWith("Query string parameter 'other' is unknown.");
        error4.Source.Should().NotBeNull();
        error4.Source.Parameter.Should().Be("other");
        error4.Meta.Should().HaveInStackTrace($"*{typeof(QueryStringReader).FullName}*");
        error4.Meta.Should().NotHaveInStackTrace($"*{typeof(QueryStringParameterReader).FullName}*");
    }
}
