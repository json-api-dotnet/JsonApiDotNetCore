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

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddSingleton<ResourceCaptureStore>();

            services.AddResourceRepository<ResultCapturingRepository<Blog, int>>();
            services.AddResourceRepository<ResultCapturingRepository<BlogPost, int>>();
            services.AddResourceRepository<ResultCapturingRepository<WebAccount, int>>();
        });
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.ManyValue[0].Relationships.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("author").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attribute_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_relationship_in_primary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("author").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Caption.Should().BeNull();
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_secondary_resources()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();
        blog.Posts = _fakers.BlogPost.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/posts?fields[blogPosts]=caption,labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[0].StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(blog.Posts[0].Caption));
        responseDocument.Data.ManyValue[0].Relationships.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Relationships.ShouldContainKey("labels").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().BeNull();

        blogCaptured.Posts.ShouldHaveCount(1);
        blogCaptured.Posts[0].Caption.Should().Be(blog.Posts[0].Caption);
        blogCaptured.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resource_by_ID()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("url").With(value => value.Should().Be(post.Url));
        responseDocument.Data.SingleValue.Relationships.ShouldHaveCount(1);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("author").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Url.Should().Be(post.Url);
        postCaptured.Caption.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_ManyToOne_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();
        post.Author = _fakers.WebAccount.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?include=author&fields[webAccounts]=displayName,emailAddress,preferences";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("author").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Id.Should().Be(post.Author.StringId);
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldHaveCount(2);
        responseDocument.Included[0].Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(post.Author.DisplayName));
        responseDocument.Included[0].Attributes.ShouldContainKey("emailAddress").With(value => value.Should().Be(post.Author.EmailAddress));
        responseDocument.Included[0].Relationships.ShouldHaveCount(1);

        responseDocument.Included[0].Relationships.ShouldContainKey("preferences").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);

        postCaptured.Author.ShouldNotBeNull();
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

        WebAccount account = _fakers.WebAccount.Generate();
        account.Posts = _fakers.BlogPost.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?include=posts&fields[blogPosts]=caption,labels";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(account.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(account.DisplayName));

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("posts").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(account.Posts[0].StringId);
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(account.Posts[0].Caption));
        responseDocument.Included[0].Relationships.ShouldHaveCount(1);

        responseDocument.Included[0].Relationships.ShouldContainKey("labels").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var accountCaptured = (WebAccount)store.Resources.Should().ContainSingle(resource => resource is WebAccount).And.Subject.Single();
        accountCaptured.Id.Should().Be(account.Id);
        accountCaptured.DisplayName.Should().Be(account.DisplayName);

        accountCaptured.Posts.ShouldHaveCount(1);
        accountCaptured.Posts[0].Caption.Should().Be(account.Posts[0].Caption);
        accountCaptured.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_OneToMany_relationship_on_secondary_endpoint()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();
        blog.Owner = _fakers.WebAccount.Generate();
        blog.Owner.Posts = _fakers.BlogPost.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}/owner?include=posts&fields[blogPosts]=caption,comments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(blog.Owner.DisplayName));

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("posts").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(blog.Owner.Posts[0].StringId);
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(blog.Owner.Posts[0].Caption));
        responseDocument.Included[0].Relationships.ShouldHaveCount(1);

        responseDocument.Included[0].Relationships.ShouldContainKey("comments").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Owner.ShouldNotBeNull();
        blogCaptured.Owner.DisplayName.Should().Be(blog.Owner.DisplayName);

        blogCaptured.Owner.Posts.ShouldHaveCount(1);
        blogCaptured.Owner.Posts[0].Caption.Should().Be(blog.Owner.Posts[0].Caption);
        blogCaptured.Owner.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_fields_of_ManyToMany_relationship()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();
        post.Labels = _fakers.Label.Generate(1).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?include=labels&fields[labels]=color";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("labels").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(post.Labels.ElementAt(0).StringId);
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldHaveCount(1);
        responseDocument.Included[0].Attributes.ShouldContainKey("color").With(value => value.Should().Be(post.Labels.Single().Color));
        responseDocument.Included[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);

        postCaptured.Labels.ShouldHaveCount(1);
        postCaptured.Labels.Single().Color.Should().Be(post.Labels.Single().Color);
        postCaptured.Labels.Single().Name.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_attributes_in_multiple_resource_types()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();
        blog.Owner = _fakers.WebAccount.Generate();
        blog.Owner.Posts = _fakers.BlogPost.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?include=owner.posts&fields[blogs]=title&fields[webAccounts]=userName,displayName&fields[blogPosts]=caption";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("blogs");
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("title").With(value => value.Should().Be(blog.Title));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        responseDocument.Included.ShouldHaveCount(2);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
        responseDocument.Included[0].Attributes.ShouldHaveCount(2);
        responseDocument.Included[0].Attributes.ShouldContainKey("userName").With(value => value.Should().Be(blog.Owner.UserName));
        responseDocument.Included[0].Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(blog.Owner.DisplayName));
        responseDocument.Included[0].Relationships.Should().BeNull();

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[0].StringId);
        responseDocument.Included[1].Attributes.ShouldHaveCount(1);
        responseDocument.Included[1].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(blog.Owner.Posts[0].Caption));
        responseDocument.Included[1].Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
        blogCaptured.Id.Should().Be(blog.Id);
        blogCaptured.Title.Should().Be(blog.Title);
        blogCaptured.PlatformName.Should().BeNull();

        blogCaptured.Owner!.UserName.Should().Be(blog.Owner.UserName);
        blogCaptured.Owner.DisplayName.Should().Be(blog.Owner.DisplayName);
        blogCaptured.Owner.DateOfBirth.Should().BeNull();

        blogCaptured.Owner.Posts.ShouldHaveCount(1);
        blogCaptured.Owner.Posts[0].Caption.Should().Be(blog.Owner.Posts[0].Caption);
        blogCaptured.Owner.Posts[0].Url.Should().BeNull();
    }

    [Fact]
    public async Task Can_select_only_top_level_fields_with_multiple_includes()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();
        blog.Owner = _fakers.WebAccount.Generate();
        blog.Owner.Posts = _fakers.BlogPost.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?include=owner.posts&fields[blogs]=title,owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("blogs");
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("title").With(value => value.Should().Be(blog.Title));
        responseDocument.Data.SingleValue.Relationships.ShouldHaveCount(1);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("owner").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included.ShouldHaveCount(2);

        responseDocument.Included[0].Type.Should().Be("webAccounts");
        responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
        responseDocument.Included[0].Attributes.ShouldContainKey("userName").With(value => value.Should().Be(blog.Owner.UserName));
        responseDocument.Included[0].Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(blog.Owner.DisplayName));
        responseDocument.Included[0].Attributes.ShouldContainKey("dateOfBirth").With(value => value.Should().Be(blog.Owner.DateOfBirth));

        responseDocument.Included[0].Relationships.ShouldContainKey("posts").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Id.Should().Be(blog.Owner.Posts[0].StringId);
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        responseDocument.Included[1].Type.Should().Be("blogPosts");
        responseDocument.Included[1].Id.Should().Be(blog.Owner.Posts[0].StringId);
        responseDocument.Included[1].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(blog.Owner.Posts[0].Caption));
        responseDocument.Included[1].Attributes.ShouldContainKey("url").With(value => value.Should().Be(blog.Owner.Posts[0].Url));

        responseDocument.Included[1].Relationships.ShouldContainKey("labels").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
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

        BlogPost post = _fakers.BlogPost.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
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

        BlogPost post = _fakers.BlogPost.Generate();

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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().BeNull();
        responseDocument.Data.ManyValue[0].Relationships.Should().BeNull();

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Url.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_select_on_unknown_resource_type()
    {
        // Arrange
        const string route = $"/webAccounts?fields[{Unknown.ResourceType}]=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified fieldset is invalid.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be($"fields[{Unknown.ResourceType}]");
    }

    [Fact]
    public async Task Cannot_select_attribute_with_blocked_capability()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.Generate();

        string route = $"/webAccounts/{account.Id}?fields[webAccounts]=password";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Retrieving the requested attribute is not allowed.");
        error.Detail.Should().Be("Retrieving the attribute 'password' is not allowed.");
        error.Source.ShouldNotBeNull();
        error.Source.Parameter.Should().Be("fields[webAccounts]");
    }

    [Fact]
    public async Task Retrieves_all_properties_when_fieldset_contains_readonly_attribute()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        Blog blog = _fakers.Blog.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Blogs.Add(blog);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogs/{blog.StringId}?fields[blogs]=showAdvertisements";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("showAdvertisements").With(value => value.Should().Be(blog.ShowAdvertisements));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        var blogCaptured = (Blog)store.Resources.Should().ContainSingle(resource => resource is Blog).And.Subject.Single();
        blogCaptured.ShowAdvertisements.Should().Be(blogCaptured.ShowAdvertisements);
        blogCaptured.Title.Should().Be(blog.Title);
    }

    [Fact]
    public async Task Can_select_fields_on_resource_type_multiple_times()
    {
        // Arrange
        var store = _testContext.Factory.Services.GetRequiredService<ResourceCaptureStore>();
        store.Clear();

        BlogPost post = _fakers.BlogPost.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/blogPosts/{post.StringId}?fields[blogPosts]=url&fields[blogPosts]=caption,url&fields[blogPosts]=caption,author";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(2);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("caption").With(value => value.Should().Be(post.Caption));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("url").With(value => value.Should().Be(post.Url));
        responseDocument.Data.SingleValue.Relationships.ShouldHaveCount(1);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("author").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.Value.Should().BeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.ShouldNotBeNull();
            value.Links.Related.ShouldNotBeNull();
        });

        var postCaptured = (BlogPost)store.Resources.Should().ContainSingle(resource => resource is BlogPost).And.Subject.Single();
        postCaptured.Id.Should().Be(post.Id);
        postCaptured.Caption.Should().Be(post.Caption);
        postCaptured.Url.Should().Be(postCaptured.Url);
    }

    [Fact]
    public async Task Returns_related_resources_on_broken_resource_linkage()
    {
        // Arrange
        WebAccount account = _fakers.WebAccount.Generate();
        account.Posts = _fakers.BlogPost.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webAccounts/{account.StringId}?include=posts&fields[webAccounts]=displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(account.StringId);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        responseDocument.Included.ShouldHaveCount(2);
        responseDocument.Included.Should().OnlyContain(resourceObject => resourceObject.Type == "blogPosts");
    }
}
