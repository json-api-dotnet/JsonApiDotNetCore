using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Sorting;

public sealed class SortTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string CollectionErrorMessage = "This query string parameter can only be used on a collection of resources (not on a single resource).";

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SortTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<WebAccountsController>();
    }

    [Fact]
    public async Task Can_sort_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(3);
        posts[0].Caption = "B";
        posts[1].Caption = "A";
        posts[2].Caption = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?sort=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(posts[2].StringId);
    }

    [Fact]
    public async Task Cannot_sort_in_primary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^sort");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_sort_in_secondary_resources()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Posts[0].Caption = "B";
        blog.Posts[1].Caption = "A";
        blog.Posts[2].Caption = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?sort=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(blog.Posts[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(blog.Posts[2].StringId);
    }

    [Fact]
    public async Task Cannot_sort_in_secondary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}/author?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^sort");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_sort_on_OneToMany_relationship()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?sort=count(posts)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(blogs[0].StringId);
    }

    [Fact]
    public async Task Can_sort_on_ManyToMany_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);
        posts[0].Labels = _fakers.Label.GenerateSet(1);
        posts[1].Labels = _fakers.Label.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?sort=-count(labels)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[0].StringId);
    }

    [Fact]
    public async Task Can_sort_in_scope_of_OneToMany_relationship()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();
        account.Posts = _fakers.BlogPost.GenerateList(3);
        account.Posts[0].Caption = "B";
        account.Posts[1].Caption = "A";
        account.Posts[2].Caption = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?include=posts&sort[posts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(account.StringId);

        responseDocument.Included.Should().HaveCount(3);
        responseDocument.Included[0].Id.Should().Be(account.Posts[1].StringId);
        responseDocument.Included[1].Id.Should().Be(account.Posts[0].StringId);
        responseDocument.Included[2].Id.Should().Be(account.Posts[2].StringId);
    }

    [Fact]
    public async Task Can_sort_in_scope_of_OneToMany_relationship_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Owner.Posts[0].Caption = "B";
        blog.Owner.Posts[1].Caption = "A";
        blog.Owner.Posts[2].Caption = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/owner?include=posts&sort[posts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);

        responseDocument.Included.Should().HaveCount(3);
        responseDocument.Included[0].Id.Should().Be(blog.Owner.Posts[1].StringId);
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[0].StringId);
        responseDocument.Included[2].Id.Should().Be(blog.Owner.Posts[2].StringId);
    }

    [Fact]
    public async Task Can_sort_in_scope_of_ManyToMany_relationship()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();
        post.Labels = _fakers.Label.GenerateSet(3);
        post.Labels.ElementAt(0).Name = "B";
        post.Labels.ElementAt(1).Name = "A";
        post.Labels.ElementAt(2).Name = "C";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?include=labels&sort[labels]=name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);

        responseDocument.Included.Should().HaveCount(3);
        responseDocument.Included[0].Id.Should().Be(post.Labels.ElementAt(1).StringId);
        responseDocument.Included[1].Id.Should().Be(post.Labels.ElementAt(0).StringId);
        responseDocument.Included[2].Id.Should().Be(post.Labels.ElementAt(2).StringId);
    }

    [Fact]
    public async Task Can_sort_on_multiple_fields_in_multiple_scopes()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Title = "Z";
        blogs[1].Title = "Y";

        blogs[0].Posts = _fakers.BlogPost.GenerateList(4);
        blogs[0].Posts[0].Caption = "B";
        blogs[0].Posts[1].Caption = "A";
        blogs[0].Posts[2].Caption = "A";
        blogs[0].Posts[3].Caption = "C";
        blogs[0].Posts[0].Url = "";
        blogs[0].Posts[1].Url = "www.some2.com";
        blogs[0].Posts[2].Url = "www.some1.com";
        blogs[0].Posts[3].Url = "";

        blogs[0].Posts[0].Comments = _fakers.Comment.GenerateSet(3);
        blogs[0].Posts[0].Comments.ElementAt(0).CreatedAt = 1.January(2015).AsUtc();
        blogs[0].Posts[0].Comments.ElementAt(1).CreatedAt = 1.January(2014).AsUtc();
        blogs[0].Posts[0].Comments.ElementAt(2).CreatedAt = 1.January(2016).AsUtc();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts.comments&sort=title&sort[posts]=caption,url&sort[posts.comments]=-createdAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(blogs[0].StringId);

        responseDocument.Included.Should().HaveCount(7);

        responseDocument.Included[0].Type.Should().Be("blogPosts");
        responseDocument.Included[0].Id.Should().Be(blogs[0].Posts[2].StringId);

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blogs[0].Posts[1].StringId);

        responseDocument.Included[2].Type.Should().Be("blogPosts");
        responseDocument.Included[2].Id.Should().Be(blogs[0].Posts[0].StringId);

        responseDocument.Included[3].Type.Should().Be("comments");
        responseDocument.Included[3].Id.Should().Be(blogs[0].Posts[0].Comments.ElementAt(2).StringId);

        responseDocument.Included[4].Type.Should().Be("comments");
        responseDocument.Included[4].Id.Should().Be(blogs[0].Posts[0].Comments.ElementAt(0).StringId);

        responseDocument.Included[5].Type.Should().Be("comments");
        responseDocument.Included[5].Id.Should().Be(blogs[0].Posts[0].Comments.ElementAt(1).StringId);

        responseDocument.Included[6].Type.Should().Be("blogPosts");
        responseDocument.Included[6].Id.Should().Be(blogs[0].Posts[3].StringId);
    }

    [Fact]
    public async Task Can_sort_on_ManyToOne_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);
        posts[0].Author = _fakers.WebAccount.GenerateOne();
        posts[1].Author = _fakers.WebAccount.GenerateOne();

        posts[0].Author!.DisplayName = "Conner";
        posts[1].Author!.DisplayName = "Smith";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?sort=-author.displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(posts[0].StringId);
    }

    [Fact]
    public async Task Can_sort_in_multiple_scopes()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[0].Title = "Cooking";
        blogs[1].Title = "Technology";

        blogs[1].Owner = _fakers.WebAccount.GenerateOne();
        blogs[1].Owner!.Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Owner!.Posts[0].Caption = "One";
        blogs[1].Owner!.Posts[1].Caption = "Two";

        blogs[1].Owner!.Posts[1].Comments = _fakers.Comment.GenerateSet(2);
        blogs[1].Owner!.Posts[1].Comments.ElementAt(0).CreatedAt = 1.January(2000).AsUtc();
        blogs[1].Owner!.Posts[1].Comments.ElementAt(0).CreatedAt = 10.January(2010).AsUtc();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=owner.posts.comments&sort=-title&sort[owner.posts]=-caption&sort[owner.posts.comments]=-createdAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blogs[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(blogs[0].StringId);

        responseDocument.Included.Should().HaveCount(5);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blogs[1].Owner!.StringId);

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blogs[1].Owner!.Posts[1].StringId);

        responseDocument.Included[2].Type.Should().Be("comments");
        responseDocument.Included[2].Id.Should().Be(blogs[1].Owner!.Posts[1].Comments.ElementAt(1).StringId);

        responseDocument.Included[3].Type.Should().Be("comments");
        responseDocument.Included[3].Id.Should().Be(blogs[1].Owner!.Posts[1].Comments.ElementAt(0).StringId);

        responseDocument.Included[4].Type.Should().Be("blogPosts");
        responseDocument.Included[4].Id.Should().Be(blogs[1].Owner!.Posts[0].StringId);
    }

    [Fact]
    public async Task Cannot_sort_in_unknown_scope()
    {
        // Arrange
        var parameterName = new MarkedText($"sort[^{Unknown.Relationship}]", '^');
        string route = $"/webAccounts?{parameterName.Text}=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'webAccounts'. {parameterName}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName.Text);
    }

    [Fact]
    public async Task Cannot_sort_in_unknown_nested_scope()
    {
        // Arrange
        var parameterName = new MarkedText($"sort[posts.^{Unknown.Relationship}]", '^');
        string route = $"/webAccounts?{parameterName.Text}=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'blogPosts'. {parameterName}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName.Text);
    }

    [Fact]
    public async Task Cannot_sort_on_attribute_with_blocked_capability()
    {
        // Arrange
        var parameterValue = new MarkedText("^dateOfBirth", '^');
        string route = $"/webAccounts?sort={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Sorting on attribute 'dateOfBirth' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_sort_descending_by_ID()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.GenerateList(3);
        accounts[0].Id = 3000;
        accounts[1].Id = 2000;
        accounts[2].Id = 1000;

        accounts[0].DisplayName = "B";
        accounts[1].DisplayName = "A";
        accounts[2].DisplayName = "A";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WebAccount>();
            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/webAccounts?sort=displayName,-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(accounts[2].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(accounts[0].StringId);
    }

    [Fact]
    public async Task Sorts_by_ID_if_none_specified()
    {
        // Arrange
        List<WebAccount> accounts = _fakers.WebAccount.GenerateList(4);
        accounts[0].Id = 300;
        accounts[1].Id = 200;
        accounts[2].Id = 100;
        accounts[3].Id = 400;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<WebAccount>();
            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/webAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);
        responseDocument.Data.ManyValue[0].Id.Should().Be(accounts[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(accounts[1].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(accounts[0].StringId);
        responseDocument.Data.ManyValue[3].Id.Should().Be(accounts[3].StringId);
    }
}
