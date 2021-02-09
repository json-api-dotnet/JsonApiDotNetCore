using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Pagination
{
    public sealed class PaginationWithTotalCountTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private const int _defaultPageSize = 5;

        private readonly ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public PaginationWithTotalCountTests(ExampleIntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;
            options.DefaultPageSize = new PageSize(_defaultPageSize);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;
            options.AllowUnknownQueryStringParameters = true;

            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_paginate_in_primary_resources()
        {
            // Arrange
            var posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);

                await dbContext.SaveChangesAsync();
            });

            var route = "/blogPosts?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?page[size]=1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_paginate_in_single_primary_resource()
        {
            // Arrange
            var post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogPosts/{post.StringId}?page[number]=2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Can_paginate_in_secondary_resources()
        {
            // Arrange
            var blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogs/{blog.StringId}/posts?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/blogs/{blog.StringId}/posts?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().Be($"http://localhost/blogs/{blog.StringId}/posts?page[number]=3&page[size]=1");
        }

        [Fact]
        public async Task Cannot_paginate_in_single_secondary_resource()
        {
            // Arrange
            var post = _fakers.BlogPost.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Posts.Add(post);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogPosts/{post.StringId}/author?page[size]=5";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task Can_paginate_in_scope_of_HasMany_relationship()
        {
            // Arrange
            var blogs = _fakers.Blog.Generate(3);
            blogs[0].Posts = _fakers.BlogPost.Generate(2);
            blogs[1].Posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);
                await dbContext.SaveChangesAsync();
            });

            var route = "/blogs?include=posts&page[number]=posts:2&page[size]=2,posts:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blogs[0].Posts[1].StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/blogs?include=posts&page[size]=2,posts:1");
            responseDocument.Links.Last.Should().Be("http://localhost/blogs?include=posts&page[number]=2&page[size]=2,posts:1");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
        }

        [Fact]
        public async Task Can_paginate_in_scope_of_HasMany_relationship_on_secondary_resource()
        {
            // Arrange
            var blog = _fakers.Blog.Generate();
            blog.Owner = _fakers.WebAccount.Generate();
            blog.Owner.Posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogs/{blog.StringId}/owner?include=posts&page[number]=posts:2&page[size]=posts:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Id.Should().Be(blog.Owner.Posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Can_paginate_HasMany_relationship_on_relationship_endpoint()
        {
            // Arrange
            var blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogs/{blog.StringId}/relationships/posts?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/blogs/{blog.StringId}/relationships/posts?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Can_paginate_in_scope_of_HasManyThrough_relationship()
        {
            // Arrange
            var posts = _fakers.BlogPost.Generate(2);
            posts[0].BlogPostLabels = new HashSet<BlogPostLabel>
            {
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                },
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                }
            };
            posts[1].BlogPostLabels = new HashSet<BlogPostLabel>
            {
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                },
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);
                await dbContext.SaveChangesAsync();
            });

            // Workaround for https://github.com/dotnet/efcore/issues/21026
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = true;
            options.DisableChildrenPagination = false;

            var route = "/blogPosts?include=labels&page[number]=labels:2&page[size]=labels:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(posts[0].BlogPostLabels.Skip(1).First().Label.StringId);
            responseDocument.Included[1].Id.Should().Be(posts[1].BlogPostLabels.Skip(1).First().Label.StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/blogPosts?include=labels&page[size]=labels:1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Can_paginate_HasManyThrough_relationship_on_relationship_endpoint()
        {
            // Arrange
            var post = _fakers.BlogPost.Generate();
            post.BlogPostLabels = new HashSet<BlogPostLabel>
            {
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                },
                new BlogPostLabel
                {
                    Label = _fakers.Label.Generate()
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.Add(post);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogPosts/{post.StringId}/relationships/labels?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(post.BlogPostLabels.ElementAt(1).Label.StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/blogPosts/{post.StringId}/relationships/labels?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Can_paginate_in_multiple_scopes()
        {
            // Arrange
            var blogs = _fakers.Blog.Generate(2);
            blogs[1].Owner = _fakers.WebAccount.Generate();
            blogs[1].Owner.Posts = _fakers.BlogPost.Generate(2);
            blogs[1].Owner.Posts[1].Comments = _fakers.Comment.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);
                await dbContext.SaveChangesAsync();
            });

            var route = "/blogs?include=owner.posts.comments&" +
                        "page[size]=1,owner.posts:1,owner.posts.comments:1&" +
                        "page[number]=2,owner.posts:2,owner.posts.comments:2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);

            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included[0].Id.Should().Be(blogs[1].Owner.StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Owner.Posts[1].StringId);
            responseDocument.Included[2].Id.Should().Be(blogs[1].Owner.Posts[1].Comments.Skip(1).First().StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/blogs?include=owner.posts.comments&page[size]=1,owner.posts:1,owner.posts.comments:1");
            responseDocument.Links.Last.Should().Be("http://localhost/blogs?include=owner.posts.comments&page[size]=1,owner.posts:1,owner.posts.comments:1&page[number]=2");
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_paginate_in_unknown_scope()
        {
            // Arrange
            var route = "/webAccounts?page[number]=doesNotExist:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'webAccounts'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_paginate_in_unknown_nested_scope()
        {
            // Arrange
            var route = "/webAccounts?page[size]=posts.doesNotExist:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'posts.doesNotExist' does not exist on resource 'blogPosts'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task Uses_default_page_number_and_size()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(2);

            var blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(3);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogs/{blog.StringId}/posts";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(blog.Posts[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blog.Posts[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().Be($"http://localhost/blogs/{blog.StringId}/posts?page[number]=2");
        }

        [Fact]
        public async Task Returns_all_resources_when_paging_is_disabled()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            var blog = _fakers.Blog.Generate();
            blog.Posts = _fakers.BlogPost.Generate(25);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/blogs/{blog.StringId}/posts";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(25);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
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
            var account = _fakers.WebAccount.Generate();
            account.UserName = "&" + account.UserName;

            const int totalCount = 3 * _defaultPageSize + 3;
            var posts = _fakers.BlogPost.Generate(totalCount);

            foreach (var post in posts)
            {
                post.Author = account;
            }

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<BlogPost>();
                dbContext.Posts.AddRange(posts);

                await dbContext.SaveChangesAsync();
            });

            var routePrefix = "/blogPosts?filter=equals(author.userName,'" + WebUtility.UrlEncode(account.UserName) + "')" +
                              "&fields[webAccounts]=userName&include=author&sort=id&foo=bar,baz";
            var route = routePrefix + $"&page[number]={pageNumber}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be("http://localhost" + route);

            if (firstLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, firstLink.Value);
                responseDocument.Links.First.Should().Be(expected);
            }
            else
            {
                responseDocument.Links.First.Should().BeNull();
            }

            if (prevLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, prevLink.Value);
                responseDocument.Links.Prev.Should().Be(expected);
            }
            else
            {
                responseDocument.Links.Prev.Should().BeNull();
            }

            if (nextLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, nextLink.Value);
                responseDocument.Links.Next.Should().Be(expected);
            }
            else
            {
                responseDocument.Links.Next.Should().BeNull();
            }

            if (lastLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, lastLink.Value);
                responseDocument.Links.Last.Should().Be(expected);
            }
            else
            {
                responseDocument.Links.Last.Should().BeNull();
            }

            static string SetPageNumberInUrl(string url, int pageNumber)
            {
                return pageNumber != 1 ? url + "&page[number]=" + pageNumber : url;
            }
        }
    }
}
