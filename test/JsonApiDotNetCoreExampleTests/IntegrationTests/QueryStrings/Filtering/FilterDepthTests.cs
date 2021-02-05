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
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterDepthTests : IClassFixture<ExampleIntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly ExampleIntegrationTestContext<Startup, AppDbContext> _testContext;

        public FilterDepthTests(ExampleIntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;

            options.DisableTopPagination = false;
            options.DisableChildrenPagination = false;
        }

        [Fact]
        public async Task Can_filter_in_primary_resources()
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

            var route = "/api/v1/articles?filter=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
        }

        [Fact]
        public async Task Cannot_filter_in_single_primary_resource()
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

            var route = $"/api/v1/articles/{article.StringId}?filter=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Can_filter_in_secondary_resources()
        {
            // Arrange
            var blog = new LegacyBlog
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

            var route = $"/api/v1/legacyBlogs/{blog.StringId}/articles?filter=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[1].StringId);
        }

        [Fact]
        public async Task Cannot_filter_in_single_secondary_resource()
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

            var route = $"/api/v1/articles/{article.StringId}/author?filter=equals(lastName,'Smith')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified filter is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("filter");
        }

        [Fact]
        public async Task Can_filter_on_HasOne_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "X",
                    Author = new Author
                    {
                        LastName = "Conner"
                    }
                },
                new Article
                {
                    Caption = "X",
                    Author = new Author
                    {
                        LastName = "Smith"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?include=author&filter=equals(author.lastName,'Smith')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.Included.Should().HaveCount(1);

            responseDocument.Included[0].Id.Should().Be(articles[1].Author.StringId);
        }

        [Fact]
        public async Task Can_filter_on_HasMany_relationship()
        {
            // Arrange
            var blogs = new List<LegacyBlog>
            {
                new LegacyBlog(),
                new LegacyBlog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "X"
                        }
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<LegacyBlog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/legacyBlogs?filter=greaterThan(count(articles),'0')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);
        }

        [Fact]
        public async Task Can_filter_on_HasManyThrough_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "X"
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
                                Name = "Hot"
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

            var route = "/api/v1/articles?filter=has(tags)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_scope_of_HasMany_relationship()
        {
            // Arrange
            var blog = new LegacyBlog
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
                await dbContext.ClearTableAsync<LegacyBlog>();
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/legacyBlogs?include=articles&filter[articles]=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.Included.Should().HaveCount(1);

            responseDocument.Included[0].Id.Should().Be(blog.Articles[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_scope_of_HasMany_relationship_on_secondary_resource()
        {
            // Arrange
            var blog = new LegacyBlog
            {
                Owner = new Author
                {
                    LastName = "X",
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

            var route = $"/api/v1/legacyBlogs/{blog.StringId}/owner?include=articles&filter[articles]=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);

            responseDocument.Included[0].Id.Should().Be(blog.Owner.Articles[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_scope_of_HasManyThrough_relationship()
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
                                Name = "Hot"
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
            options.DisableTopPagination = false;
            options.DisableChildrenPagination = true;

            var route = "/api/v1/articles?include=tags&filter[tags]=equals(name,'Hot')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(1);

            responseDocument.Included[0].Id.Should().Be(articles[1].ArticleTags.First().Tag.StringId);
        }

        [Fact]
        public async Task Can_filter_in_scope_of_relationship_chain()
        {
            // Arrange
            var blog = new LegacyBlog
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
                await dbContext.ClearTableAsync<LegacyBlog>();
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/legacyBlogs?include=owner.articles&filter[owner.articles]=equals(caption,'Two')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blog.Owner.StringId);
            responseDocument.Included[1].Id.Should().Be(blog.Owner.Articles[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_same_scope_multiple_times()
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
                },
                new Article
                {
                    Caption = "Three"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?filter=equals(caption,'One')&filter=equals(caption,'Three')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(articles[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[2].StringId);
        }

        [Fact]
        public async Task Can_filter_in_same_scope_multiple_times_using_legacy_notation()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = true;

            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "One",
                    Author = new Author
                    {
                        FirstName = "Joe",
                        LastName = "Smith"
                    }
                },
                new Article
                {
                    Caption = "Two",
                    Author = new Author
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    }
                },
                new Article
                {
                    Caption = "Three",
                    Author = new Author
                    {
                        FirstName = "Jack",
                        LastName = "Miller"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?filter[author.firstName]=John&filter[author.lastName]=Smith";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(articles[0].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[1].StringId);
        }

        [Fact]
        public async Task Can_filter_in_multiple_scopes()
        {
            // Arrange
            var blogs = new List<LegacyBlog>
            {
                new LegacyBlog(),
                new LegacyBlog
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
                await dbContext.ClearTableAsync<LegacyBlog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/legacyBlogs?include=owner.articles.revisions&" +
                        "filter=and(equals(title,'Technology'),has(owner.articles),equals(owner.lastName,'Smith'))&" +
                        "filter[owner.articles]=equals(caption,'Two')&" +
                        "filter[owner.articles.revisions]=greaterThan(publishTime,'2005-05-05')";

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
    }
}
