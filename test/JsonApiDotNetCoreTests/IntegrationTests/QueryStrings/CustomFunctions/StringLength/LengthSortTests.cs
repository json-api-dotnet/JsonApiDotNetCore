using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

public sealed class LengthSortTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public LengthSortTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<ISortParser, LengthSortParser>();
            services.AddTransient<IOrderClauseBuilder, LengthOrderClauseBuilder>();
        });
    }

    [Fact]
    public async Task Can_sort_on_length_at_primary_endpoint()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);

        blogs[0].Title = "X";
        blogs[1].Title = "XXX";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?sort=-length(title)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[1].Id.Should().Be(blogs[0].StringId);
    }

    [Fact]
    public async Task Can_sort_on_length_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);

        blog.Posts[0].Caption = "XXX";
        blog.Posts[0].Url = "YYY";

        blog.Posts[1].Caption = "XXX";
        blog.Posts[1].Url = "Y";

        blog.Posts[2].Caption = "X";
        blog.Posts[2].Url = "Y";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?sort=length(caption),length(url)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[2].StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[1].Id.Should().Be(blog.Posts[1].StringId);

        responseDocument.Data.ManyValue[2].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[2].Id.Should().Be(blog.Posts[0].StringId);
    }

    [Fact]
    public async Task Can_sort_on_length_in_included_resources()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Title = "XXX";
        blogs[1].Title = "X";

        blogs[1].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Posts[0].Caption = "YYY";
        blogs[1].Posts[1].Caption = "Y";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?sort=length(title)&include=posts&sort[posts]=length(caption)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);

        responseDocument.Data.ManyValue[1].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[1].Id.Should().Be(blogs[0].StringId);

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blogs[1].Posts[1].StringId);

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blogs[1].Posts[0].StringId);
    }
}
