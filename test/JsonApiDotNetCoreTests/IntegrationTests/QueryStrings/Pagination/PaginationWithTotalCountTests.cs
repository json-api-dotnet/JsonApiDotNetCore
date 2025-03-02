using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Pagination;

public sealed class PaginationWithTotalCountTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private const string HostPrefix = "http://localhost";
    private const int DefaultPageSize = 5;
    private const string CollectionErrorMessage = "This query string parameter can only be used on a collection of resources (not on a single resource).";

    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public PaginationWithTotalCountTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<WebAccountsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;
        options.DefaultPageSize = new PageSize(DefaultPageSize);
        options.MaximumPageSize = null;
        options.MaximumPageNumber = null;
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogPosts?page%5Bnumber%5D=2&page%5Bsize%5D=1");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_paginate_in_primary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?page[number]=2";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^page[number]");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Can_paginate_in_secondary_resources()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(5);

        Blog otherBlog = _fakers.Blog.GenerateOne();
        otherBlog.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.AddRange(blog, otherBlog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?page[number]=3&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[2].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bnumber%5D=5&page%5Bsize%5D=1");
        responseDocument.Links.Prev.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bnumber%5D=2&page%5Bsize%5D=1");
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/posts?page%5Bnumber%5D=4&page%5Bsize%5D=1");
    }

    [Fact]
    public async Task Can_paginate_in_secondary_resources_without_inverse_relationship()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();
        account.LoginAttempts = _fakers.LoginAttempt.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}/loginAttempts?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(account.LoginAttempts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/webAccounts/{account.StringId}/loginAttempts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/webAccounts/{account.StringId}/loginAttempts?page%5Bnumber%5D=3&page%5Bsize%5D=1");
    }

    [Fact]
    public async Task Cannot_paginate_in_secondary_resource()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}/author?page[size]=5";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"{CollectionErrorMessage} Failed at position 1: ^page[size]");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Can_paginate_in_scope_of_OneToMany_relationship()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(3);
        blogs[0].Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Posts = _fakers.BlogPost.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=posts&page[number]=posts:2&page[size]=2,posts:1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Id.Should().Be(blogs[0].Posts[1].StringId);
        responseDocument.Included[1].Id.Should().Be(blogs[1].Posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogs?include=posts&page%5Bsize%5D=2,posts%3A1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogs?include=posts&page%5Bnumber%5D=2&page%5Bsize%5D=2,posts%3A1");
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Can_paginate_in_scope_of_OneToMany_relationship_at_secondary_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/owner?include=posts&page[number]=posts:2&page[size]=posts:1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(blog.Owner.Posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Can_paginate_OneToMany_relationship_at_relationship_endpoint()
    {
        // Arrange
        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(4);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/relationships/posts?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[1].StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/relationships/posts?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/relationships/posts?page%5Bnumber%5D=4&page%5Bsize%5D=1");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/blogs/{blog.StringId}/relationships/posts?page%5Bnumber%5D=3&page%5Bsize%5D=1");
    }

    [Fact]
    public async Task Can_paginate_OneToMany_relationship_at_relationship_endpoint_without_inverse_relationship()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();
        account.LoginAttempts = _fakers.LoginAttempt.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}/relationships/loginAttempts?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(account.LoginAttempts[1].StringId);

        string basePath = $"{HostPrefix}/webAccounts/{account.StringId}/relationships/loginAttempts";

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{basePath}?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be($"{basePath}?page%5Bnumber%5D=3&page%5Bsize%5D=1");
    }

    [Fact]
    public async Task Can_paginate_in_scope_of_ManyToMany_relationship()
    {
        // Arrange
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(2);
        posts[0].Labels = _fakers.Label.GenerateSet(2);
        posts[1].Labels = _fakers.Label.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?include=labels&page[number]=labels:2&page[size]=labels:1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Id.Should().Be(posts[0].Labels.ElementAt(1).StringId);
        responseDocument.Included[1].Id.Should().Be(posts[1].Labels.ElementAt(1).StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts?include=labels&page%5Bsize%5D=labels%3A1");
        responseDocument.Links.Last.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Can_paginate_ManyToMany_relationship_at_relationship_endpoint()
    {
        // Arrange
        BlogPost post = _fakers.BlogPost.GenerateOne();
        post.Labels = _fakers.Label.GenerateSet(4);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}/relationships/labels?page[number]=2&page[size]=1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.Labels.ElementAt(1).StringId);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{HostPrefix}/blogPosts/{post.StringId}/relationships/labels?page%5Bsize%5D=1");
        responseDocument.Links.Last.Should().Be($"{HostPrefix}/blogPosts/{post.StringId}/relationships/labels?page%5Bnumber%5D=4&page%5Bsize%5D=1");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().Be($"{HostPrefix}/blogPosts/{post.StringId}/relationships/labels?page%5Bnumber%5D=3&page%5Bsize%5D=1");
    }

    [Fact]
    public async Task Can_paginate_in_multiple_scopes()
    {
        // Arrange
        List<Blog> blogs = _fakers.Blog.GenerateList(2);
        blogs[1].Owner = _fakers.WebAccount.GenerateOne();
        blogs[1].Owner!.Posts = _fakers.BlogPost.GenerateList(2);
        blogs[1].Owner!.Posts[1].Comments = _fakers.Comment.GenerateSet(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Blog>();
            dbContext.Blogs.AddRange(blogs);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogs?include=owner.posts.comments&page[size]=1,owner.posts:1,owner.posts.comments:1&" +
            "page[number]=2,owner.posts:2,owner.posts.comments:2";

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

        const string linkPrefix = $"{HostPrefix}/blogs?include=owner.posts.comments";

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be($"{linkPrefix}&page%5Bsize%5D=1,owner.posts%3A1,owner.posts.comments%3A1");
        responseDocument.Links.Last.Should().Be($"{linkPrefix}&page%5Bsize%5D=1,owner.posts%3A1,owner.posts.comments%3A1&page%5Bnumber%5D=2");
        responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
        responseDocument.Links.Next.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_paginate_in_unknown_scope()
    {
        // Arrange
        var parameterValue = new MarkedText($"^{Unknown.Relationship}:1", '^');
        string route = $"/webAccounts?page[number]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'webAccounts'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[number]");
    }

    [Fact]
    public async Task Cannot_paginate_in_unknown_nested_scope()
    {
        // Arrange
        var parameterValue = new MarkedText($"posts.^{Unknown.Relationship}:1", '^');
        string route = $"/webAccounts?page[size]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified pagination is invalid.");
        error.Detail.Should().Be($"Field '{Unknown.Relationship}' does not exist on resource type 'blogPosts'. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("page[size]");
    }

    [Fact]
    public async Task Uses_default_page_number_and_size()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = new PageSize(2);

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(3);
        blog.Posts.ForEach(post => post.Labels = _fakers.Label.GenerateSet(3));

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?include=labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[0].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(blog.Posts[1].StringId);

        responseDocument.Included.Should().HaveCount(4);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
        responseDocument.Links.Last.Should().Be($"{responseDocument.Links.Self}&page%5Bnumber%5D=2");
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
    }

    [Fact]
    public async Task Returns_all_resources_when_pagination_is_disabled()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.DefaultPageSize = null;

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(25);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(25);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");
        responseDocument.Links.First.Should().BeNull();
        responseDocument.Links.Last.Should().BeNull();
        responseDocument.Links.Prev.Should().BeNull();
        responseDocument.Links.Next.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 1, 4, null, 2)]
    [InlineData(2, 1, 4, 1, 3)]
    [InlineData(3, 1, 4, 2, 4)]
    [InlineData(4, 1, 4, 3, null)]
    public async Task Renders_correct_top_level_links_for_page_number(int pageNumber, int? firstLink, int? lastLink, int? prevLink, int? nextLink)
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();

        const int totalCount = 3 * DefaultPageSize + 3;
        List<BlogPost> posts = _fakers.BlogPost.GenerateList(totalCount);

        foreach (BlogPost post in posts)
        {
            post.Author = account;
        }

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
        });

        string routePrefix = $"/blogPosts?filter=equals(author.userName,'{account.UserName}')" +
            "&fields[webAccounts]=userName&include=author&sort=id&foo=bar,baz";

        string route = $"{routePrefix}&page[number]={pageNumber}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().NotBeNull();
        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be($"{HostPrefix}{route}");

        if (firstLink != null)
        {
            string expected = $"{HostPrefix}{SetPageNumberInUrl(routePrefix, firstLink.Value)}";
            responseDocument.Links.First.Should().Be(expected);
        }
        else
        {
            responseDocument.Links.First.Should().BeNull();
        }

        if (prevLink != null)
        {
            string expected = $"{HostPrefix}{SetPageNumberInUrl(routePrefix, prevLink.Value)}";
            responseDocument.Links.Prev.Should().Be(expected);
        }
        else
        {
            responseDocument.Links.Prev.Should().BeNull();
        }

        if (nextLink != null)
        {
            string expected = $"{HostPrefix}{SetPageNumberInUrl(routePrefix, nextLink.Value)}";
            responseDocument.Links.Next.Should().Be(expected);
        }
        else
        {
            responseDocument.Links.Next.Should().BeNull();
        }

        if (lastLink != null)
        {
            string expected = $"{HostPrefix}{SetPageNumberInUrl(routePrefix, lastLink.Value)}";
            responseDocument.Links.Last.Should().Be(expected);
        }
        else
        {
            responseDocument.Links.Last.Should().BeNull();
        }

        static string SetPageNumberInUrl(string url, int pageNumber)
        {
            string link = pageNumber != 1 ? $"{url}&page[number]={pageNumber}" : url;
            return link.Replace("[", "%5B").Replace("]", "%5D").Replace("'", "%27");
        }
    }
}
