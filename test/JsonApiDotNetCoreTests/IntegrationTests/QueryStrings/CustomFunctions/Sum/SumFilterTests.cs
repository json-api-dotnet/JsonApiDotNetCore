using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

public sealed class SumFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SumFilterTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, SumFilterParser>();
            services.AddTransient<IWhereClauseBuilder, SumWhereClauseBuilder>();
        });
    }

    [Fact]
    public async Task Can_filter_sum_at_primary_endpoint()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        posts[0].Comments = _fakers.Comment.GenerateSet(2);
        posts[0].Comments.ElementAt(0).NumStars = 0;
        posts[0].Comments.ElementAt(1).NumStars = 1;

        posts[1].Comments = _fakers.Comment.GenerateSet(2);
        posts[1].Comments.ElementAt(0).NumStars = 2;
        posts[1].Comments.ElementAt(1).NumStars = 3;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=greaterThan(sum(comments,numStars),'4')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_sum_on_count_at_secondary_endpoint()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        posts[0].Comments = _fakers.Comment.GenerateSet(2);
        posts[0].Comments.ElementAt(0).NumStars = 1;
        posts[0].Comments.ElementAt(1).NumStars = 1;
        posts[0].Contributors = _fakers.Woman.GenerateSet<Woman, Human>(1);

        posts[1].Comments = _fakers.Comment.GenerateSet(2);
        posts[1].Comments.ElementAt(0).NumStars = 2;
        posts[1].Comments.ElementAt(1).NumStars = 2;
        posts[1].Contributors = _fakers.Man.GenerateSet<Man, Human>(2);
        posts[1].Contributors.ElementAt(0).Children = _fakers.Woman.GenerateSet<Woman, Human>(3);
        posts[1].Contributors.ElementAt(1).Children = _fakers.Man.GenerateSet<Man, Human>(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=lessThan(sum(comments,numStars),sum(contributors,count(children)))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogPosts");
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_sum_in_included_resources()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(2);

        blog.Posts[0].Comments = _fakers.Comment.GenerateSet(2);
        blog.Posts[0].Comments.ElementAt(0).NumStars = 1;
        blog.Posts[0].Comments.ElementAt(1).NumStars = 1;

        blog.Posts[1].Comments = _fakers.Comment.GenerateSet(2);
        blog.Posts[1].Comments.ElementAt(0).NumStars = 1;
        blog.Posts[1].Comments.ElementAt(1).NumStars = 2;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&filter[posts]=equals(sum(comments,numStars),'3')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("blogs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.StringId);

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blog.Posts[1].StringId);
    }
}
