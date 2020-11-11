using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Pagination
{
    public sealed class PaginationWithTotalCountTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private const int _defaultPageSize = 5;

        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;
        private readonly Faker<TodoItem> _todoItemFaker = new Faker<TodoItem>();

        public PaginationWithTotalCountTests(IntegrationTestContext<Startup, AppDbContext> testContext)
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

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?page[size]=1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
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

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/api/v1/blogs/{blog.StringId}/articles?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().Be($"http://localhost/api/v1/blogs/{blog.StringId}/articles?page[number]=3&page[size]=1");
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
                },
                new Blog()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/blogs?include=articles&page[number]=articles:2&page[size]=2,articles:1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.Included.Should().HaveCount(2);

            responseDocument.Included[0].Id.Should().Be(blogs[0].Articles[1].StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Articles[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/blogs?include=articles&page[size]=2,articles:1");
            responseDocument.Links.Last.Should().Be("http://localhost/api/v1/blogs?include=articles&page[number]=2&page[size]=2,articles:1");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().Be(responseDocument.Links.Last);
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

            var route = $"/api/v1/blogs/{blog.StringId}/relationships/articles?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[1].StringId);

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/api/v1/blogs/{blog.StringId}/relationships/articles?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
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

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/articles?include=tags&page[size]=tags:1");
            responseDocument.Links.Last.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();
        }

        [Fact]
        public async Task Can_paginate_HasManyThrough_relationship_on_relationship_endpoint()
        {
            // Arrange
            var article = new Article
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
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}/relationships/tags?page[number]=2&page[size]=1";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(article.ArticleTags.ElementAt(1).TagId.ToString());

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be($"http://localhost/api/v1/articles/{article.StringId}/relationships/tags?page[size]=1");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
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

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be("http://localhost/api/v1/blogs?include=owner.articles.revisions&page[size]=1,owner.articles:1,owner.articles.revisions:1");
            responseDocument.Links.Last.Should().Be("http://localhost/api/v1/blogs?include=owner.articles.revisions&page[size]=1,owner.articles:1,owner.articles.revisions:1&page[number]=2");
            responseDocument.Links.Prev.Should().Be(responseDocument.Links.First);
            responseDocument.Links.Next.Should().BeNull();
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

            responseDocument.Links.Should().NotBeNull();
            responseDocument.Links.Self.Should().Be("http://localhost" + route);
            responseDocument.Links.First.Should().Be(responseDocument.Links.Self);
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().Be($"http://localhost/api/v1/blogs/{blog.StringId}/articles?page[number]=2");
        }

        [Fact]
        public async Task Returns_all_resources_when_paging_is_disabled()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.DefaultPageSize = null;

            var blog = new Blog
            {
                Articles = new List<Article>()
            };

            for (int index = 0; index < 25; index++)
            {
                blog.Articles.Add(new Article
                {
                    Caption = $"Item {index:D3}"
                });
            }

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
            var person = new Person
            {
                LastName = "&Ampersand"
            };

            const int totalCount = 3 * _defaultPageSize + 3;
            var todoItems = _todoItemFaker.Generate(totalCount);
            
            foreach (var todoItem in todoItems)
            {
                todoItem.Owner = person;
            }

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<TodoItem>();
                dbContext.TodoItems.AddRange(todoItems);

                await dbContext.SaveChangesAsync();
            });

            var routePrefix = "/api/v1/todoItems?filter=equals(owner.lastName,'" + WebUtility.UrlEncode(person.LastName) + "')" +
                        $"&fields[owner]=firstName&include=owner&sort=ordinal&foo=bar,baz";
            var route = routePrefix + $"&page[number]={pageNumber}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            Assert.Equal("http://localhost" + route, responseDocument.Links.Self);

            if (firstLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, firstLink.Value);
                Assert.Equal(expected, responseDocument.Links.First);
            }
            else
            {
                Assert.Null(responseDocument.Links.First);
            }

            if (prevLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, prevLink.Value);
                Assert.Equal(expected, responseDocument.Links.Prev);
            }
            else
            {
                Assert.Null(responseDocument.Links.Prev);
            }

            if (nextLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, nextLink.Value);
                Assert.Equal(expected, responseDocument.Links.Next);
            }
            else
            {
                Assert.Null(responseDocument.Links.Next);
            }

            if (lastLink != null)
            {
                var expected = "http://localhost" + SetPageNumberInUrl(routePrefix, lastLink.Value);
                Assert.Equal(expected, responseDocument.Links.Last);
            }
            else
            {
                Assert.Null(responseDocument.Links.Last);
            }

            static string SetPageNumberInUrl(string url, int pageNumber)
            {
                return pageNumber != 1 ? url + "&page[number]=" + pageNumber : url;
            }
        }
    }
}
