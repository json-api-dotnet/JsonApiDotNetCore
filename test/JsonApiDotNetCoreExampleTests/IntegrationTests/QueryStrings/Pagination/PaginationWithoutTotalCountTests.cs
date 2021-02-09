using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Pagination
{
    public sealed class PaginationWithoutTotalCountTests 
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private const int _defaultPageSize = 5;

        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public PaginationWithoutTotalCountTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();

            options.IncludeTotalResourceCount = false;
            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.AllowUnknownQueryStringParameters = true;
        }

        [Fact]
        public async Task Hides_pagination_links_when_unconstrained_page_size()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            var route = "/blogPosts?foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/blogPosts?foo=bar");
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_size_is_specified_in_query_string_with_no_data()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                await dbContext.SaveChangesAsync();
            });

            var route = "/blogPosts?page[size]=8&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/blogPosts?page[size]=8&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?page[size]=8&foo=bar");
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

            var route = "/blogPosts?page[number]=2&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/blogPosts?page[number]=2&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/blogPosts?foo=bar");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_partially_filled_page()
        {
            // Arrange
            var posts = _fakers.BlogPost.Generate(12);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);
                await dbContext.SaveChangesAsync();
            });

            var route = "/blogPosts?foo=bar&page[number]=3";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Count.Should().BeLessThan(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/blogPosts?foo=bar&page[number]=3");
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/blogPosts?foo=bar&page[number]=2");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page()
        {
            // Arrange
            var posts = _fakers.BlogPost.Generate(_defaultPageSize * 3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);
                await dbContext.SaveChangesAsync();
            });

            var route = "/blogPosts?page[number]=3&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/blogPosts?page[number]=3&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/blogPosts?page[number]=2&foo=bar");
            responseDocument.Links.Next.Should().Be("http://localhost/blogPosts?page[number]=4&foo=bar");
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page_on_secondary_endpoint()
        {
            // Arrange
            var account = _fakers.WebAccount.Generate();
            account.Posts = _fakers.BlogPost.Generate(_defaultPageSize * 3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Accounts.Add(account);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/webAccounts/{account.StringId}/posts?page[number]=3&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be($"http://localhost/webAccounts/{account.StringId}/posts?page[number]=3&foo=bar");
            responseDocument.Links.First.Should().Be($"http://localhost/webAccounts/{account.StringId}/posts?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be($"http://localhost/webAccounts/{account.StringId}/posts?page[number]=2&foo=bar");
            responseDocument.Links.Next.Should().Be($"http://localhost/webAccounts/{account.StringId}/posts?page[number]=4&foo=bar");
        }
    }
}
