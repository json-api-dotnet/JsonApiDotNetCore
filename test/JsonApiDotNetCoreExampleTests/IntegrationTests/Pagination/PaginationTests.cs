using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Pagination
{
    public sealed class PaginationTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public PaginationTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(5);
            options.MaximumPageSize = null;
            options.MaximumPageNumber = null;

            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_paginate_in_primary_resources()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "One"
                },
                new Article
                {
                    Caption = "Two"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
        }

        [Fact]
        public async Task Cannot_paginate_in_single_primary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?page[number]=2";

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
            var blog = new Blog
            {
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "One"
                    },
                    new Article
                    {
                        Caption = "Two"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[1].StringId);
        }

        [Fact]
        public async Task Cannot_paginate_in_single_secondary_resource()
        {
            // Arrange
            var article = new Article
            {
                Caption = "X"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}/author?page[size]=5";

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
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One"
                        },
                        new Article
                        {
                            Caption = "Two"
                        }
                    }
                },
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "First"
                        },
                        new Article
                        {
                            Caption = "Second"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/blogs?include=articles&page[number]=articles:2&page[size]=articles:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blogs[0].Articles[1].StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Articles[1].StringId);
        }

        [Fact]
        public async Task Can_paginate_in_scope_of_HasMany_relationship_on_secondary_resource()
        {
            // Arrange
            var blog = new Blog
            {
                Owner = new Author
                {
                    LastName = "Smith",
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "One"
                        },
                        new Article
                        {
                            Caption = "Two"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/owner?include=articles&page[number]=articles:2&page[size]=articles:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Id.Should().Be(blog.Owner.Articles[1].StringId);
        }

        [Fact]
        public async Task Can_paginate_in_scope_of_HasManyThrough_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "X",
                    ArticleTags = new HashSet<ArticleTag>
                    {
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "Cold"
                            }
                        },
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "Hot"
                            }
                        }
                    }
                },
                new Article
                {
                    Caption = "X",
                    ArticleTags = new HashSet<ArticleTag>
                    {
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "Wet"
                            }
                        },
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "Dry"
                            }
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            // Workaround for https://github.com/dotnet/efcore/issues/21026
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DisableTopPagination = true;
            options.DisableChildrenPagination = false;

            var route = "/api/v1/articles?include=tags&page[number]=tags:2&page[size]=tags:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(articles[0].ArticleTags.Skip(1).First().Tag.StringId);
            responseDocument.Included[1].Id.Should().Be(articles[1].ArticleTags.Skip(1).First().Tag.StringId);
        }

        [Fact]
        public async Task Can_paginate_in_multiple_scopes()
        {
            // Arrange
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Title = "Cooking"
                },
                new Blog
                {
                    Title = "Technology",
                    Owner = new Author
                    {
                        LastName = "Smith",
                        Articles = new List<Article>
                        {
                            new Article
                            {
                                Caption = "One"
                            },
                            new Article
                            {
                                Caption = "Two",
                                Revisions = new List<Revision>
                                {
                                    new Revision
                                    {
                                        PublishTime = 1.January(2000)
                                    },
                                    new Revision
                                    {
                                        PublishTime = 10.January(2010)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/blogs?include=owner.articles.revisions&" +
                        "page[size]=1,owner.articles:1,owner.articles.revisions:1&" +
                        "page[number]=2,owner.articles:2,owner.articles.revisions:2";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);

            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included[0].Id.Should().Be(blogs[1].Owner.StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Owner.Articles[1].StringId);
            responseDocument.Included[2].Id.Should().Be(blogs[1].Owner.Articles[1].Revisions.Skip(1).First().StringId);
        }

        [Fact]
        public async Task Cannot_paginate_in_unknown_scope()
        {
            // Arrange
            var route = "/api/v1/people?page[number]=doesNotExist:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'people'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[number]");
        }

        [Fact]
        public async Task Cannot_paginate_in_unknown_nested_scope()
        {
            // Arrange
            var route = "/api/v1/people?page[size]=todoItems.doesNotExist:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified paging is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'todoItems.doesNotExist' does not exist on resource 'todoItems'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("page[size]");
        }

        [Fact]
        public async Task Uses_default_page_number_and_size()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = new PageSize(2);

            var blog = new Blog
            {
                Articles = new List<Article>
                {
                    new Article
                    {
                        Caption = "One"
                    },
                    new Article
                    {
                        Caption = "Two"
                    },
                    new Article
                    {
                        Caption = "Three"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blog.Articles[1].StringId);
        }
    }
}
