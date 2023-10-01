using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Pagination;

public sealed class RangeValidationTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const int DefaultPageSize = 5;
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public RangeValidationTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = new PageSize(DefaultPageSize);
        options.MaximumPageSize = null;
        options.MaximumPageNumber = null;
    }

    [Fact]
    public async Task Cannot_use_negative_page_number()
    {
        // Arrange
        var parameterValue = new MarkedText("^-1", '^');
        string route = $"/blogs?page[number]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Page number cannot be negative or zero. {parameterValue}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Cannot_use_zero_page_number()
    {
        // Arrange
        var parameterValue = new MarkedText("^0", '^');
        string route = $"/blogs?page[number]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Page number cannot be negative or zero. {parameterValue}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Can_use_positive_page_number()
    {
        // Arrange
        const string route = "/blogs?page[number]=20";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_empty_set_of_resources_when_page_number_is_too_high()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.Generate(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?sort=id&page[size]=3&page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_use_negative_page_size()
    {
        // Arrange
        var parameterValue = new MarkedText("^-1", '^');
        string route = $"/blogs?page[size]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Page size cannot be negative. {parameterValue}");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Can_use_zero_page_size()
    {
        // Arrange
        const string route = "/blogs?page[size]=0";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Can_use_positive_page_size()
    {
        // Arrange
        const string route = "/blogs?page[size]=50";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
