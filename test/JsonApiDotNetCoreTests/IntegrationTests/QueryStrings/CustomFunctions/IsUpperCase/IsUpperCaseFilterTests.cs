using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

public sealed class IsUpperCaseFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public IsUpperCaseFilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, IsUpperCaseFilterParser>();
            services.AddTransient<IWhereClauseBuilder, IsUpperCaseWhereClauseBuilder>();
        });
    }

    [Fact]
    public async Task Can_filter_casing_at_primary_endpoint()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);

        blogs[0].Title = blogs[0].Title.ToLowerInvariant();
        blogs[1].Title = blogs[1].Title.ToUpperInvariant();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=isUpperCase(title)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
    }

    [Fact]
    public async Task Can_filter_casing_in_compound_expression_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(4);

        blog.Posts[0].Caption = blog.Posts[0].Caption.ToUpperInvariant();
        blog.Posts[0].Url = blog.Posts[0].Url.ToUpperInvariant();

        blog.Posts[1].Caption = blog.Posts[1].Caption.ToUpperInvariant();
        blog.Posts[1].Url = blog.Posts[1].Url.ToLowerInvariant();

        blog.Posts[2].Caption = blog.Posts[1].Caption.ToLowerInvariant();
        blog.Posts[2].Url = blog.Posts[1].Url.ToUpperInvariant();

        blog.Posts[3].Caption = blog.Posts[2].Caption.ToLowerInvariant();
        blog.Posts[3].Url = blog.Posts[2].Url.ToLowerInvariant();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?filter=and(isUpperCase(caption),not(isUpperCase(url)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_casing_in_included_resources()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Title = blogs[0].Title.ToLowerInvariant();
        blogs[1].Title = blogs[1].Title.ToUpperInvariant();

        blogs[1].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Posts[0].Caption = blogs[1].Posts[0].Caption.ToLowerInvariant();
        blogs[1].Posts[1].Caption = blogs[1].Posts[1].Caption.ToUpperInvariant();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=isUpperCase(title)&include=posts&filter[posts]=isUpperCase(caption)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blogs[1].Posts[1].StringId);
    }
}
