using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Includes
{
    public sealed class IncludeTests : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new();

        public IncludeTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<BlogPostsController>();
            testContext.UseController<BlogsController>();
            testContext.UseController<CommentsController>();
            testContext.UseController<WebAccountsController>();

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = null;
        }

        [Fact]
        public async Task Can_include_in_primary_resources()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();
            post.Author = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            const string route = "blogPosts?include=author";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Id.Should().Be(post.StringId);
            responseDocument.Data.ManyValue[0].Attributes["caption"].Should().Be(post.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(post.Author.StringId);
            responseDocument.Included[0].Attributes["displayName"].Should().Be(post.Author.DisplayName);
        }

        [Fact]
        public async Task Can_include_in_primary_resource_by_ID()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();
            post.Author = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            string route = $"blogPosts/{post.StringId}?include=author";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
            responseDocument.Data.SingleValue.Attributes["caption"].Should().Be(post.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(post.Author.StringId);
            responseDocument.Included[0].Attributes["displayName"].Should().Be(post.Author.DisplayName);
        }

        [Fact]
        public async Task Can_include_in_secondary_resource()
        {
            // Arrange
            Blog blog = _fakers.Blog.Generate();
            blog.Owner = _fakers.WebAccount.Generate();
            blog.Owner.Posts = _fakers.BlogPost.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogs/{blog.StringId}/owner?include=posts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(blog.Owner.StringId);
            responseDocument.Data.SingleValue.Attributes["displayName"].Should().Be(blog.Owner.DisplayName);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(blog.Owner.Posts[0].StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Owner.Posts[0].Caption);
        }

        [Fact]
        public async Task Can_include_in_secondary_resources()
        {
            // Arrange
            Blog blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(1);
            blog.Posts[0].Author = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogs/{blog.StringId}/posts?include=author";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Id.Should().Be(blog.Posts[0].StringId);
            responseDocument.Data.ManyValue[0].Attributes["caption"].Should().Be(blog.Posts[0].Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(blog.Posts[0].Author.StringId);
            responseDocument.Included[0].Attributes["displayName"].Should().Be(blog.Posts[0].Author.DisplayName);
        }

        [Fact]
        public async Task Can_include_ToOne_relationships()
        {
            // Arrange
            Comment comment = _fakers.Comment.Generate();
            comment.Author = _fakers.WebAccount.Generate();
            comment.Parent = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/comments/{comment.StringId}?include=author,parent";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(comment.StringId);
            responseDocument.Data.SingleValue.Attributes["text"].Should().Be(comment.Text);

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(comment.Author.StringId);
            responseDocument.Included[0].Attributes["userName"].Should().Be(comment.Author.UserName);

            responseDocument.Included[1].Type.Should().Be("blogPosts");
            responseDocument.Included[1].Id.Should().Be(comment.Parent.StringId);
            responseDocument.Included[1].Attributes["caption"].Should().Be(comment.Parent.Caption);
        }

        [Fact]
        public async Task Can_include_OneToMany_relationship()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();
            post.Comments = _fakers.Comment.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            string route = $"blogPosts/{post.StringId}?include=comments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
            responseDocument.Data.SingleValue.Attributes["caption"].Should().Be(post.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("comments");
            responseDocument.Included[0].Id.Should().Be(post.Comments.Single().StringId);
            responseDocument.Included[0].Attributes["createdAt"].As<DateTime>().Should().BeCloseTo(post.Comments.Single().CreatedAt);
        }

        [Fact]
        public async Task Can_include_ManyToMany_relationship()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();
            post.Labels = _fakers.Label.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            string route = $"blogPosts/{post.StringId}?include=labels";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
            responseDocument.Data.SingleValue.Attributes["caption"].Should().Be(post.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("labels");
            responseDocument.Included[0].Id.Should().Be(post.Labels.Single().StringId);
            responseDocument.Included[0].Attributes["name"].Should().Be(post.Labels.Single().Name);
        }

        [Fact]
        public async Task Can_include_ManyToMany_relationship_on_secondary_endpoint()
        {
            // Arrange
            BlogPost post = _fakers.BlogPost.Generate();
            post.Labels = _fakers.Label.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogPosts/{post.StringId}/labels?include=posts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().HaveCount(1);
            responseDocument.Data.ManyValue[0].Type.Should().Be("labels");
            responseDocument.Data.ManyValue[0].Id.Should().Be(post.Labels.ElementAt(0).StringId);
            responseDocument.Data.ManyValue[0].Attributes["name"].Should().Be(post.Labels.Single().Name);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(post.StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(post.Caption);
        }

        [Fact]
        public async Task Can_include_chain_of_ToOne_relationships()
        {
            // Arrange
            Comment comment = _fakers.Comment.Generate();
            comment.Parent = _fakers.BlogPost.Generate();
            comment.Parent.Author = _fakers.WebAccount.Generate();
            comment.Parent.Author.Preferences = _fakers.AccountPreferences.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync();
            });

            string route = $"comments/{comment.StringId}?include=parent.author.preferences";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(comment.StringId);
            responseDocument.Data.SingleValue.Attributes["text"].Should().Be(comment.Text);

            responseDocument.Included.Should().HaveCount(3);

            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(comment.Parent.StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(comment.Parent.Caption);

            responseDocument.Included[1].Type.Should().Be("webAccounts");
            responseDocument.Included[1].Id.Should().Be(comment.Parent.Author.StringId);
            responseDocument.Included[1].Attributes["displayName"].Should().Be(comment.Parent.Author.DisplayName);

            responseDocument.Included[2].Type.Should().Be("accountPreferences");
            responseDocument.Included[2].Id.Should().Be(comment.Parent.Author.Preferences.StringId);
            responseDocument.Included[2].Attributes["useDarkTheme"].Should().Be(comment.Parent.Author.Preferences.UseDarkTheme);
        }

        [Fact]
        public async Task Can_include_chain_of_OneToMany_relationships()
        {
            // Arrange
            Blog blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(1);
            blog.Posts[0].Comments = _fakers.Comment.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogs/{blog.StringId}?include=posts.comments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
            responseDocument.Data.SingleValue.Attributes["title"].Should().Be(blog.Title);

            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(blog.Posts[0].StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Posts[0].Caption);

            responseDocument.Included[1].Type.Should().Be("comments");
            responseDocument.Included[1].Id.Should().Be(blog.Posts[0].Comments.Single().StringId);
            responseDocument.Included[1].Attributes["createdAt"].As<DateTime>().Should().BeCloseTo(blog.Posts[0].Comments.Single().CreatedAt);
        }

        [Fact]
        public async Task Can_include_chain_of_recursive_relationships()
        {
            // Arrange
            Comment comment = _fakers.Comment.Generate();
            comment.Parent = _fakers.BlogPost.Generate();
            comment.Parent.Author = _fakers.WebAccount.Generate();
            comment.Parent.Comments = _fakers.Comment.Generate(2).ToHashSet();
            comment.Parent.Comments.ElementAt(0).Author = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/comments/{comment.StringId}?include=parent.comments.author";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(comment.StringId);
            responseDocument.Data.SingleValue.Attributes["text"].Should().Be(comment.Text);

            responseDocument.Included.Should().HaveCount(5);

            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(comment.Parent.StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(comment.Parent.Caption);

            responseDocument.Included[1].Type.Should().Be("comments");
            responseDocument.Included[1].Id.Should().Be(comment.StringId);
            responseDocument.Included[1].Attributes["text"].Should().Be(comment.Text);

            responseDocument.Included[2].Type.Should().Be("comments");
            responseDocument.Included[2].Id.Should().Be(comment.Parent.Comments.ElementAt(0).StringId);
            responseDocument.Included[2].Attributes["text"].Should().Be(comment.Parent.Comments.ElementAt(0).Text);

            responseDocument.Included[3].Type.Should().Be("webAccounts");
            responseDocument.Included[3].Id.Should().Be(comment.Parent.Comments.ElementAt(0).Author.StringId);
            responseDocument.Included[3].Attributes["userName"].Should().Be(comment.Parent.Comments.ElementAt(0).Author.UserName);

            responseDocument.Included[4].Type.Should().Be("comments");
            responseDocument.Included[4].Id.Should().Be(comment.Parent.Comments.ElementAt(1).StringId);
            responseDocument.Included[4].Attributes["text"].Should().Be(comment.Parent.Comments.ElementAt(1).Text);
        }

        [Fact]
        public async Task Can_include_chain_of_relationships_with_multiple_paths()
        {
            // Arrange
            Blog blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(1);
            blog.Posts[0].Author = _fakers.WebAccount.Generate();
            blog.Posts[0].Author.Preferences = _fakers.AccountPreferences.Generate();
            blog.Posts[0].Comments = _fakers.Comment.Generate(2).ToHashSet();
            blog.Posts[0].Comments.ElementAt(0).Author = _fakers.WebAccount.Generate();
            blog.Posts[0].Comments.ElementAt(0).Author.Posts = _fakers.BlogPost.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogs/{blog.StringId}?include=posts.author.preferences,posts.comments.author.posts";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(blog.StringId);
            responseDocument.Data.SingleValue.Attributes["title"].Should().Be(blog.Title);

            responseDocument.Included.Should().HaveCount(7);

            responseDocument.Included[0].Type.Should().Be("blogPosts");
            responseDocument.Included[0].Id.Should().Be(blog.Posts[0].StringId);
            responseDocument.Included[0].Attributes["caption"].Should().Be(blog.Posts[0].Caption);

            responseDocument.Included[1].Type.Should().Be("webAccounts");
            responseDocument.Included[1].Id.Should().Be(blog.Posts[0].Author.StringId);
            responseDocument.Included[1].Attributes["userName"].Should().Be(blog.Posts[0].Author.UserName);

            responseDocument.Included[2].Type.Should().Be("accountPreferences");
            responseDocument.Included[2].Id.Should().Be(blog.Posts[0].Author.Preferences.StringId);
            responseDocument.Included[2].Attributes["useDarkTheme"].Should().Be(blog.Posts[0].Author.Preferences.UseDarkTheme);

            responseDocument.Included[3].Type.Should().Be("comments");
            responseDocument.Included[3].Id.Should().Be(blog.Posts[0].Comments.ElementAt(0).StringId);
            responseDocument.Included[3].Attributes["text"].Should().Be(blog.Posts[0].Comments.ElementAt(0).Text);

            responseDocument.Included[4].Type.Should().Be("webAccounts");
            responseDocument.Included[4].Id.Should().Be(blog.Posts[0].Comments.ElementAt(0).Author.StringId);
            responseDocument.Included[4].Attributes["userName"].Should().Be(blog.Posts[0].Comments.ElementAt(0).Author.UserName);

            responseDocument.Included[5].Type.Should().Be("blogPosts");
            responseDocument.Included[5].Id.Should().Be(blog.Posts[0].Comments.ElementAt(0).Author.Posts[0].StringId);
            responseDocument.Included[5].Attributes["caption"].Should().Be(blog.Posts[0].Comments.ElementAt(0).Author.Posts[0].Caption);

            responseDocument.Included[6].Type.Should().Be("comments");
            responseDocument.Included[6].Id.Should().Be(blog.Posts[0].Comments.ElementAt(1).StringId);
            responseDocument.Included[6].Attributes["text"].Should().Be(blog.Posts[0].Comments.ElementAt(1).Text);
        }

        [Fact]
        public async Task Prevents_duplicate_includes_over_single_resource()
        {
            // Arrange
            WebAccount account = _fakers.WebAccount.Generate();

            BlogPost post = _fakers.BlogPost.Generate();
            post.Author = account;
            post.Reviewer = account;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogPosts/{post.StringId}?include=author&include=reviewer";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.SingleValue.Should().NotBeNull();
            responseDocument.Data.SingleValue.Id.Should().Be(post.StringId);
            responseDocument.Data.SingleValue.Attributes["caption"].Should().Be(post.Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(account.StringId);
            responseDocument.Included[0].Attributes["userName"].Should().Be(account.UserName);
        }

        [Fact]
        public async Task Prevents_duplicate_includes_over_multiple_resources()
        {
            // Arrange
            WebAccount account = _fakers.WebAccount.Generate();

            List<BlogPost> posts = _fakers.BlogPost.Generate(2);
            posts[0].Author = account;
            posts[1].Author = account;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/blogPosts?include=author";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().HaveCount(2);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(account.StringId);
            responseDocument.Included[0].Attributes["userName"].Should().Be(account.UserName);
        }

        [Fact]
        public async Task Cannot_include_unknown_relationship()
        {
            // Arrange
            string route = $"/webAccounts?include={Unknown.Relationship}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified include is invalid.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' does not exist on resource 'webAccounts'.");
            error.Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Cannot_include_unknown_nested_relationship()
        {
            // Arrange
            string route = $"/blogs?include=posts.{Unknown.Relationship}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified include is invalid.");
            error.Detail.Should().Be($"Relationship '{Unknown.Relationship}' in 'posts.{Unknown.Relationship}' does not exist on resource 'blogPosts'.");
            error.Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Cannot_include_relationship_with_blocked_capability()
        {
            // Arrange
            const string route = "/blogPosts?include=parent";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Including the requested relationship is not allowed.");
            error.Detail.Should().Be("Including the relationship 'parent' on 'blogPosts' is not allowed.");
            error.Source.Parameter.Should().Be("include");
        }

        [Fact]
        public async Task Ignores_null_parent_in_nested_include()
        {
            // Arrange
            List<BlogPost> posts = _fakers.BlogPost.Generate(2);
            posts[0].Reviewer = _fakers.WebAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/blogPosts?include=reviewer.preferences";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.Should().HaveCount(2);

            ResourceObject[] postWithReviewer = responseDocument.Data.ManyValue
                .Where(resource => resource.Relationships.First(pair => pair.Key == "reviewer").Value.Data.SingleValue != null).ToArray();

            postWithReviewer.Should().HaveCount(1);
            postWithReviewer[0].Attributes["caption"].Should().Be(posts[0].Caption);

            ResourceObject[] postWithoutReviewer = responseDocument.Data.ManyValue
                .Where(resource => resource.Relationships.First(pair => pair.Key == "reviewer").Value.Data.SingleValue == null).ToArray();

            postWithoutReviewer.Should().HaveCount(1);
            postWithoutReviewer[0].Attributes["caption"].Should().Be(posts[1].Caption);

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("webAccounts");
            responseDocument.Included[0].Id.Should().Be(posts[0].Reviewer.StringId);
            responseDocument.Included[0].Attributes["userName"].Should().Be(posts[0].Reviewer.UserName);
        }

        [Fact]
        public async Task Can_include_at_configured_maximum_inclusion_depth()
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = 1;

            Blog blog = _fakers.Blog.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/blogs/{blog.StringId}/posts?include=author,comments";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_exceed_configured_maximum_inclusion_depth()
        {
            // Arrange
            var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.MaximumIncludeDepth = 1;

            const string route = "/blogs/123/owner?include=posts.comments";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("The specified include is invalid.");
            error.Detail.Should().Be("Including 'posts.comments' exceeds the maximum inclusion depth of 1.");
            error.Source.Parameter.Should().Be("include");
        }
    }
}
