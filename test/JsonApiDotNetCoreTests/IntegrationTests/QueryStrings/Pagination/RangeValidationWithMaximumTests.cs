using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Pagination;

public sealed class RangeValidationWithMaximumTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const int MaximumPageSize = 15;
    private const int MaximumPageNumber = 20;
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;

    public RangeValidationWithMaximumTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = new PageSize(5);
        options.MaximumPageSize = new PageSize(MaximumPageSize);
        options.MaximumPageNumber = new PageNumber(MaximumPageNumber);
    }

    [Fact]
    public async Task Can_use_page_number_below_maximum()
    {
        // Arrange
        const int pageNumber = MaximumPageNumber - 1;
        string route = $"/blogs?page[number]={pageNumber}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Can_use_page_number_equal_to_maximum()
    {
        // Arrange
        const int pageNumber = MaximumPageNumber;
        string route = $"/blogs?page[number]={pageNumber}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cannot_use_page_number_over_maximum()
    {
        // Arrange
        const int pageNumber = MaximumPageNumber + 1;
        string route = $"/blogs?page[number]={pageNumber}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified paging is invalid.");
        error.Detail.Should().Be($"Page number cannot be higher than {MaximumPageNumber}.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Cannot_use_zero_page_size()
    {
        // Arrange
        const string route = "/blogs?page[size]=0";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified paging is invalid.");
        error.Detail.Should().Be("Page size cannot be unconstrained.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Can_use_page_size_below_maximum()
    {
        // Arrange
        const int pageSize = MaximumPageSize - 1;
        string route = $"/blogs?page[size]={pageSize}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Can_use_page_size_equal_to_maximum()
    {
        // Arrange
        const int pageSize = MaximumPageSize;
        string route = $"/blogs?page[size]={pageSize}";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cannot_use_page_size_over_maximum()
    {
        // Arrange
        const int pageSize = MaximumPageSize + 1;
        string route = $"/blogs?page[size]={pageSize}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified paging is invalid.");
        error.Detail.Should().Be($"Page size cannot be higher than {MaximumPageSize}.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }
}
