using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.SparseFieldSets;

public sealed class SparseFieldSetTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly QueryStringFakers _fakers = new();

    public SparseFieldSetTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<BlogPostsController>();
        testContext.UseController<WebAccountsController>();
        testContext.UseController<BlogsController>();
        testContext.UseController<CalendarsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceRepository<ResultCapturingRepository<Blog, int>>();
            services.AddResourceRepository<ResultCapturingRepository<BlogPost, int>>();
            services.AddResourceRepository<ResultCapturingRepository<WebAccount, int>>();

            services.AddSingleton<ResourceCaptureStore>();
        });
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=caption,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);
        responseDocument.Data.ManyValue[0].Relationships.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("author").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_relationship_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("author").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Caption.Should().BeNull();
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?fields[blogPosts]=caption,labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(blog.Posts[0].Caption);
        responseDocument.Data.ManyValue[0].Relationships.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("labels").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().BeNull();

        blogCaptured.Posts.Should().HaveCount(1);
        blogCaptured.Posts[0].Caption.Should().Be(blog.Posts[0].Caption);
        blogCaptured.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resource_by_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("url").WhoseValue.Should().Be(post.Url);
        responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("author").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Url.Should().Be(post.Url);
        postCaptured.Caption.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_ManyToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();
        post.Author = _fakers.WebAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?include=author&fields[webAccounts]=displayName,emailAddress,preferences";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("author").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Id.Should().Be(post.Author.StringId);
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(post.Author.DisplayName);
        responseDocument.Included[0].Attributes.Should().ContainKey("emailAddress").WhoseValue.Should().Be(post.Author.EmailAddress);
        responseDocument.Included[0].Relationships.Should().HaveCount(1);

        responseDocument.Included[0].Relationships.Should().ContainKey("preferences").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);

        postCaptured.Author.Should().NotBeNull();
        postCaptured.Author.DisplayName.Should().Be(post.Author.DisplayName);
        postCaptured.Author.EmailAddress.Should().Be(post.Author.EmailAddress);
        postCaptured.Author.UserName.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_OneToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        WebAccount account = _fakers.WebAccount.GenerateOne();
        account.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?include=posts&fields[blogPosts]=caption,labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(account.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(account.DisplayName);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("posts").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(account.Posts[0].StringId);
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(account.Posts[0].Caption);
        responseDocument.Included[0].Relationships.Should().HaveCount(1);

        responseDocument.Included[0].Relationships.Should().ContainKey("labels").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var accountCaptured = (WebAccount)store.Resources.Should().ContainSingle(resource => resource is WebAccount).Which;
        accountCaptured.Id.Should().Be(account.Id);
        accountCaptured.DisplayName.Should().Be(account.DisplayName);

        accountCaptured.Posts.Should().HaveCount(1);
        accountCaptured.Posts[0].Caption.Should().Be(account.Posts[0].Caption);
        accountCaptured.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_OneToMany_relationship_at_secondary_endpoint()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/owner?include=posts&fields[blogPosts]=caption,comments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(blog.Owner.DisplayName);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("posts").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(blog.Owner.Posts[0].StringId);
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(blog.Owner.Posts[0].Caption);
        responseDocument.Included[0].Relationships.Should().HaveCount(1);

        responseDocument.Included[0].Relationships.Should().ContainKey("comments").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Owner.Should().NotBeNull();
        blogCaptured.Owner.DisplayName.Should().Be(blog.Owner.DisplayName);

        blogCaptured.Owner.Posts.Should().HaveCount(1);
        blogCaptured.Owner.Posts[0].Caption.Should().Be(blog.Owner.Posts[0].Caption);
        blogCaptured.Owner.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_ManyToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();
        post.Labels = _fakers.Label.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?include=labels&fields[labels]=color";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("labels").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(post.Labels.ElementAt(0).StringId);
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().HaveCount(1);
        responseDocument.Included[0].Attributes.Should().ContainKey("color").WhoseValue.Should().Be(post.Labels.Single().Color);
        responseDocument.Included[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);

        postCaptured.Labels.Should().HaveCount(1);
        postCaptured.Labels.Single().Color.Should().Be(post.Labels.Single().Color);
        postCaptured.Labels.Single().Name.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attributes_in_multiple_resource_types()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?include=owner.posts&fields[blogs]=title&fields[webAccounts]=userName,displayName&fields[blogPosts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("blogs");
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(blog.Title);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
        responseDocument.Included[0].Attributes.Should().HaveCount(2);
        responseDocument.Included[0].Attributes.Should().ContainKey("userName").WhoseValue.Should().Be(blog.Owner.UserName);
        responseDocument.Included[0].Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(blog.Owner.DisplayName);
        responseDocument.Included[0].Relationships.Should().BeNull();

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[0].StringId);
        responseDocument.Included[1].Attributes.Should().HaveCount(1);
        responseDocument.Included[1].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(blog.Owner.Posts[0].Caption);
        responseDocument.Included[1].Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().Be(blog.Title);
        blogCaptured.PlatformName.Should().BeNull();

        blogCaptured.Owner!.UserName.Should().Be(blog.Owner.UserName);
        blogCaptured.Owner.DisplayName.Should().Be(blog.Owner.DisplayName);
        blogCaptured.Owner.DateOfBirth.Should().BeNull();

        blogCaptured.Owner.Posts.Should().HaveCount(1);
        blogCaptured.Owner.Posts[0].Caption.Should().Be(blog.Owner.Posts[0].Caption);
        blogCaptured.Owner.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_only_top_level_fields_with_multiple_includes()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.Owner = _fakers.WebAccount.GenerateOne();
        blog.Owner.Posts = _fakers.BlogPost.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?include=owner.posts&fields[blogs]=title,owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("blogs");
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("title").WhoseValue.Should().Be(blog.Title);
        responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("owner").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("userName").WhoseValue.Should().Be(blog.Owner.UserName);
        responseDocument.Included[0].Attributes.Should().ContainKey("displayName").WhoseValue.Should().Be(blog.Owner.DisplayName);
        responseDocument.Included[0].Attributes.Should().ContainKey("dateOfBirth").WhoseValue.Should().Be(blog.Owner.DateOfBirth);

        responseDocument.Included[0].Relationships.Should().ContainKey("posts").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.ManyValue.Should().HaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(blog.Owner.Posts[0].StringId);
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[0].StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(blog.Owner.Posts[0].Caption);
        responseDocument.Included[1].Attributes.Should().ContainKey("url").WhoseValue.Should().Be(blog.Owner.Posts[0].Url);

        responseDocument.Included[1].Relationships.Should().ContainKey("labels").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().Be(blog.Title);
        blogCaptured.PlatformName.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=id,caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_empty_fieldset()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BlogPost>();
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/blogPosts?fields[blogPosts]=";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_on_unknown_resource_type()
    {
        // Arrange
        var parameterName = new MarkedText($"fields[^{Unknown.ResourceType}]", '^');
        string route = $"/webAccounts?{parameterName.Text}=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist. {parameterName}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be(parameterName.Text);
    }

    [Fact]
    public async Task Cannot_select_attribute_with_blocked_capability()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();

        var parameterValue = new MarkedText("^password", '^');
        string route = $"/webAccounts/{account.Id}?fields[webAccounts]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Retrieving the attribute 'password' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[webAccounts]");
    }

    [Fact]
    public async Task Cannot_select_ToOne_relationship_with_blocked_capability()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();

        var parameterValue = new MarkedText("^person", '^');
        string route = $"/webAccounts/{account.Id}?fields[webAccounts]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Retrieving the relationship 'person' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[webAccounts]");
    }

    [Fact]
    public async Task Cannot_select_ToMany_relationship_with_blocked_capability()
    {
        // Arrange
        Calendar calendar = _fakers.Calendar.GenerateOne();

        var parameterValue = new MarkedText("^appointments", '^');
        string route = $"/calendars/{calendar.Id}?fields[calendars]={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Retrieving the relationship 'appointments' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("fields[calendars]");
    }

    [Fact]
    public async Task Fetches_all_scalar_properties_when_fieldset_contains_readonly_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.GenerateOne();
        blog.IsPublished = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?fields[blogs]=showAdvertisements";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("showAdvertisements").WhoseValue.Should().Be(blog.ShowAdvertisements);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).Which;
        blogCaptured.ShowAdvertisements.Should().Be(blog.ShowAdvertisements);
        blogCaptured.IsPublished.Should().Be(blog.IsPublished);
        blogCaptured.Title.Should().Be(blog.Title);
    }

    [Fact]
    public async Task Can_select_fields_on_resource_type_multiple_times()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url&fields[blogPosts]=caption,url&fields[blogPosts]=caption,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(2);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("caption").WhoseValue.Should().Be(post.Caption);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("url").WhoseValue.Should().Be(post.Url);
        responseDocument.Data.SingleValue.Relationships.Should().HaveCount(1);

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("author").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().NotBeNull();
            value.Links.Related.Should().NotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).Which;
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().Be(post.Url);
    }

    [Fact]
    public async Task Returns_related_resources_on_broken_resource_linkage()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.GenerateOne();
        account.Posts = _fakers.BlogPost.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?include=posts&fields[webAccounts]=displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(account.StringId);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().OnlyContain(resource => resource.Type == "blogPosts");
    }
}
