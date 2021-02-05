using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Pagination
{
    public sealed class PaginationWithoutTotalCountTests : IClassFixture<ExampleIntegrationTestContext<Startup, AppDbContext>>
    {
        private const int _defaultPageSize = 5;

        private readonly ExampleIntegrationTestContext<Startup, AppDbContext> _testContext;
        private readonly ExampleFakers _fakers = new ExampleFakers();

        public PaginationWithoutTotalCountTests(ExampleIntegrationTestContext<Startup, AppDbContext> testContext)
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

            var route = "/api/v1/articles?foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?foo=bar");
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
                await dbContext.ClearTableAsync<Article>();
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?page[size]=8&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[size]=8&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?page[size]=8&foo=bar");
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
                await dbContext.ClearTableAsync<Article>();
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?page[number]=2&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[number]=2&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_partially_filled_page()
        {
            // Arrange
            var articles = _fakers.Article.Generate(12);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?foo=bar&page[number]=3";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Count.Should().BeLessThan(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?foo=bar&page[number]=3");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?foo=bar&page[number]=2");
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page()
        {
            // Arrange
            var articles = _fakers.Article.Generate(_defaultPageSize * 3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?page[number]=3&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost/api/v1/articles?page[number]=3&foo=bar");
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be("http://localhost/api/v1/articles?page[number]=2&foo=bar");
            responseDocument.Links.Next.Should().Be("http://localhost/api/v1/articles?page[number]=4&foo=bar");
        }

        [Fact]
        public async Task Renders_pagination_links_when_page_number_is_specified_in_query_string_with_full_page_on_secondary_endpoint()
        {
            // Arrange
            var author = new Author
            {
                Articles = _fakers.Article.Generate(_defaultPageSize * 3)
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AuthorDifferentDbContextName.Add(author);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/authors/{author.StringId}/articles?page[number]=3&foo=bar";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(_defaultPageSize);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be($"http://localhost/api/v1/authors/{author.StringId}/articles?page[number]=3&foo=bar");
            responseDocument.Links.First.Should().Be($"http://localhost/api/v1/authors/{author.StringId}/articles?foo=bar");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be($"http://localhost/api/v1/authors/{author.StringId}/articles?page[number]=2&foo=bar");
            responseDocument.Links.Next.Should().Be($"http://localhost/api/v1/authors/{author.StringId}/articles?page[number]=4&foo=bar");
        }
    }
}
