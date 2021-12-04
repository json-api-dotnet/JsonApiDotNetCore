using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Pagination;

public sealed class PaginationWithoutTotalCountTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string HostPrefix = "http://localhost";
    private const int DefaultPageSize = 5;

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public PaginationWithoutTotalCountTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<WebAccountsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = false;
        options.DefaultPageSize = new PageSize(DefaultPageSize);
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Hides_pagination_links_when_unconstrained_page_size()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = null;

        const string route = "/blogPosts?foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Renders_pagination_links_when_page_size_is_specified_in_query_string_with_no_data()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = null;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[size]=8&foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?page%5Bsize%5D=8&foo=bar");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_no_data()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[number]=2&foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?foo=bar");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_partially_filled_page()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.Generate(12);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?foo=bar&page[number]=3";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCountLessThan(DefaultPageSize);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?foo=bar");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be($"{HostPrefix}/blogPosts?foo=bar&page%5Bnumber%5D=2");
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.Generate(DefaultPageSize * 3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[number]=3&foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(DefaultPageSize);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?foo=bar");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be($"{HostPrefix}/blogPosts?page%5Bnumber%5D=2&foo=bar");
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/blogPosts?page%5Bnumber%5D=4&foo=bar");
    }

    [Fact]
    public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page_on_secondary_endpoint()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.Generate();
        account.Posts = _fakers.BlogPost.Generate(DefaultPageSize * 3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}/posts?page[number]=3&foo=bar";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(DefaultPageSize);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/webAccounts/{account.StringId}/posts?foo=bar");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be($"{HostPrefix}/webAccounts/{account.StringId}/posts?page%5Bnumber%5D=2&foo=bar");
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/webAccounts/{account.StringId}/posts?page%5Bnumber%5D=4&foo=bar");
    }
}
