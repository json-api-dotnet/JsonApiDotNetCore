using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterDepthTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string CollectionErrorMessage = "This query string parameter can only be used on a collection of resources (not on a single resource).";

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public FilterDepthTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogsController>();
        testContext.UseController<BlogPostsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);
        posts[0].Caption = "One";
        posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Cannot_filter_in_primary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?filter=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^filter");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Can_filter_in_secondary_resources()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(2);
        blog.Posts[0].Caption = "One";
        blog.Posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?filter=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[1].StringId);
    }

    [Fact]
    public async Task Cannot_filter_in_secondary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}/author?filter=equals(displayName,'John Smith')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^filter");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Can_filter_on_ManyToOne_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(3);
        posts[0].Author = _fakers.WebAccount.GenerateOne();
        posts[0].Author!.UserName = "Conner";
        posts[1].Author = _fakers.WebAccount.GenerateOne();
        posts[1].Author!.UserName = "Smith";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?include=author&filter=or(equals(author.userName,'Smith'),equals(author,null))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().ContainSingle(post => post.Id == posts[1].StringId);
        responseDocument.Data.ManyValue.Should().ContainSingle(post => post.Id == posts[2].StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(posts[1].Author!.StringId);
    }

    [Fact]
    public async Task Can_filter_on_OneToMany_relationship()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=greaterThan(count(posts),'0')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_OneToMany_relationship_with_nested_condition()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Posts = _fakers.BlogPost.GenerateList(1);
        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);
        blogs[1].Posts[0].Comments = _fakers.Comment.GenerateSet(1);
        blogs[1].Posts[0].Comments.ElementAt(0).Text = "ABC";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=has(posts,has(comments,startsWith(text,'A')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_ManyToMany_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);
        posts[1].Labels = _fakers.Label.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=has(labels)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_ManyToMany_relationship_with_nested_condition()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);

        blogs[0].Posts = _fakers.BlogPost.GenerateList(1);

        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);
        blogs[1].Posts[0].Labels = _fakers.Label.GenerateSet(1);
        blogs[1].Posts[0].Labels.ElementAt(0).Color = LabelColor.Green;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?filter=has(posts,has(labels,equals(color,'Green')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
    }

    [Fact]
    public async Task Can_filter_in_scope_of_OneToMany_relationship()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(2);
        blog.Posts[0].Caption = "One";
        blog.Posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&filter[posts]=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(blog.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_in_scope_of_OneToMany_relationship_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(2);
        blog.Owner.Posts[0].Caption = "One";
        blog.Owner.Posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/owner?include=posts&filter[posts]=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(blog.Owner.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_in_scope_of_ManyToMany_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        posts[0].Labels = _fakers.Label.GenerateSet(1);
        posts[0].Labels.ElementAt(0).Name = "Cold";

        posts[1].Labels = _fakers.Label.GenerateSet(1);
        posts[1].Labels.ElementAt(0).Name = "Hot";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?include=labels&filter[labels]=equals(name,'Hot')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(posts[1].Labels.First().StringId);
    }

    [Fact]
    public async Task Can_filter_in_scope_of_relationship_chain()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(2);
        blog.Owner.Posts[0].Caption = "One";
        blog.Owner.Posts[1].Caption = "Two";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=owner.posts&filter[owner.posts]=equals(caption,'Two')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_in_same_scope_multiple_times()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(3);
        posts[0].Caption = "One";
        posts[1].Caption = "Two";
        posts[2].Caption = "Three";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter=equals(caption,'One')&filter=equals(caption,'Three')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[2].StringId);
    }

    [Fact]
    public async Task Can_filter_in_same_scope_multiple_times_using_legacy_notation()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = true;

        List<BlogPost> posts = _fakers.BlogPost.GenerateList(3);
        posts[0].Author = _fakers.WebAccount.GenerateOne();
        posts[1].Author = _fakers.WebAccount.GenerateOne();
        posts[2].Author = _fakers.WebAccount.GenerateOne();

        posts[0].Author!.UserName = "Joe";
        posts[0].Author!.DisplayName = "Smith";

        posts[1].Author!.UserName = "John";
        posts[1].Author!.DisplayName = "Doe";

        posts[2].Author!.UserName = "Jack";
        posts[2].Author!.DisplayName = "Miller";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?filter[author.userName]=John&filter[author.displayName]=Smith";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[1].StringId);
    }

    [Fact]
    public async Task Can_filter_in_multiple_scopes()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[1].Title = "Technology";
        blogs[1].Owner = _fakers.WebAccount.GenerateOne();
        blogs[1].Owner!.UserName = "Smith";
        blogs[1].Owner!.Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Owner!.Posts[0].Caption = "One";
        blogs[1].Owner!.Posts[1].Caption = "Two";
        blogs[1].Owner!.Posts[1].Comments = _fakers.Comment.GenerateSet(2);
        blogs[1].Owner!.Posts[1].Comments.ElementAt(0).CreatedAt = 1.January(2000).AsUtc();
        blogs[1].Owner!.Posts[1].Comments.ElementAt(1).CreatedAt = 10.January(2010).AsUtc();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        // @formatter:keep_existing_linebreaks true

        const string route = "/blogs?include=owner.posts.comments&" +
            "filter=and(equals(title,'Technology'),has(owner.posts),equals(owner.userName,'Smith'))&" +
            "filter[owner.posts]=equals(caption,'Two')&" +
            "filter[owner.posts.comments]=greaterThan(createdAt,'2005-05-05Z')";

        // @formatter:keep_existing_linebreaks restore

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);

        responseDocument.Included.Should().HaveCount(3);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blogs[1].Owner!.StringId);

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blogs[1].Owner!.Posts[1].StringId);

        responseDocument.Included[2].Type.Should().Be("comments");
        responseDocument.Included[2].Id.Should().Be(blogs[1].Owner!.Posts[1].Comments.ElementAt(1).StringId);
    }
}
