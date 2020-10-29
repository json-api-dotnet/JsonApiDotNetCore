using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Sorting
{
    public sealed class SortTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;
        private readonly Faker<Article> _articleFaker;
        private readonly Faker<Author> _authorFaker;

        public SortTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            _articleFaker = new Faker<Article>()
                .RuleFor(a => a.Caption, f => f.Random.AlphaNumeric(10));

            _authorFaker = new Faker<Author>()
                .RuleFor(a => a.LastName, f => f.Random.Words(2));
        }

        [Fact]
        public async Task Can_sort_in_primary_resources()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article {Caption = "B"},
                new Article {Caption = "A"},
                new Article {Caption = "C"}
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                dbContext.Articles.AddRange(articles);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/articles?sort=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(articles[2].StringId);
        }

        [Fact]
        public async Task Cannot_sort_in_single_primary_resource()
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

            var route = $"/api/v1/articles/{article.StringId}?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Can_sort_in_secondary_resources()
        {
            // Arrange
            var blog = new Blog
            {
                Articles = new List<Article>
                {
                    new Article {Caption = "B"},
                    new Article {Caption = "A"},
                    new Article {Caption = "C"}
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/articles?sort=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(blog.Articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blog.Articles[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(blog.Articles[2].StringId);
        }

        [Fact]
        public async Task Cannot_sort_in_single_secondary_resource()
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

            var route = $"/api/v1/articles/{article.StringId}/author?sort=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("This query string parameter can only be used on a collection of resources (not on a single resource).");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Can_sort_on_HasMany_relationship()
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
                            Caption = "A"
                        },
                        new Article
                        {
                            Caption = "B"
                        }
                    }
                },
                new Blog
                {
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "C"
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

            var route = "/api/v1/blogs?sort=count(articles)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blogs[0].StringId);
        }

        [Fact]
        public async Task Can_sort_on_HasManyThrough_relationship()
        {
            // Arrange
            var articles = new List<Article>
            {
                new Article
                {
                    Caption = "First",
                    ArticleTags = new HashSet<ArticleTag>
                    {
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "A"
                            }
                        }
                    }
                },
                new Article
                {
                    Caption = "Second",
                    ArticleTags = new HashSet<ArticleTag>
                    {
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "B"
                            }
                        },
                        new ArticleTag
                        {
                            Tag = new Tag
                            {
                                Name = "C"
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

            var route = "/api/v1/articles?sort=-count(tags)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[0].StringId);
        }

        [Fact]
        public async Task Can_sort_in_scope_of_HasMany_relationship()
        {
            // Arrange
            var author = _authorFaker.Generate();
            author.Articles = new List<Article>
            {
                new Article {Caption = "B"},
                new Article {Caption = "A"},
                new Article {Caption = "C"}
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AuthorDifferentDbContextName.Add(author);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/authors/{author.StringId}?include=articles&sort[articles]=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(author.StringId);

            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included[0].Id.Should().Be(author.Articles[1].StringId);
            responseDocument.Included[1].Id.Should().Be(author.Articles[0].StringId);
            responseDocument.Included[2].Id.Should().Be(author.Articles[2].StringId);
        }

        [Fact]
        public async Task Can_sort_in_scope_of_HasMany_relationship_on_secondary_resource()
        {
            // Arrange
            var blog = new Blog
            {
                Owner = new Author
                {
                    LastName = "Smith",
                    Articles = new List<Article>
                    {
                        new Article {Caption = "B"},
                        new Article {Caption = "A"},
                        new Article {Caption = "C"}
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Blogs.Add(blog);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/blogs/{blog.StringId}/owner?include=articles&sort[articles]=caption";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(blog.Owner.StringId);

            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included[0].Id.Should().Be(blog.Owner.Articles[1].StringId);
            responseDocument.Included[1].Id.Should().Be(blog.Owner.Articles[0].StringId);
            responseDocument.Included[2].Id.Should().Be(blog.Owner.Articles[2].StringId);
        }

        [Fact]
        public async Task Can_sort_in_scope_of_HasManyThrough_relationship()
        {
            // Arrange
            var article = _articleFaker.Generate();
            article.ArticleTags = new HashSet<ArticleTag>
            {
                new ArticleTag
                {
                    Tag = new Tag
                    {
                        Name = "B"
                    }
                },
                new ArticleTag
                {
                    Tag = new Tag
                    {
                        Name = "A"
                    }
                },
                new ArticleTag
                {
                    Tag = new Tag
                    {
                        Name = "C"
                    }
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Articles.Add(article);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.StringId}?include=tags&sort[tags]=name";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(article.StringId);

            responseDocument.Included.Should().HaveCount(3);
            responseDocument.Included[0].Id.Should().Be(article.ArticleTags.Skip(1).First().Tag.StringId);
            responseDocument.Included[1].Id.Should().Be(article.ArticleTags.Skip(0).First().Tag.StringId);
            responseDocument.Included[2].Id.Should().Be(article.ArticleTags.Skip(2).First().Tag.StringId);
        }

        [Fact]
        public async Task Can_sort_on_multiple_fields_in_multiple_scopes()
        {
            // Arrange
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Title = "Z",
                    Articles = new List<Article>
                    {
                        new Article
                        {
                            Caption = "B",
                            Revisions = new List<Revision>
                            {
                                new Revision
                                {
                                    PublishTime = 1.January(2015)
                                },
                                new Revision
                                {
                                    PublishTime = 1.January(2014)
                                },
                                new Revision
                                {
                                    PublishTime = 1.January(2016)
                                }
                            }
                        },
                        new Article
                        {
                            Caption = "A",
                            Url = "www.some2.com"
                        },
                        new Article
                        {
                            Caption = "A",
                            Url = "www.some1.com"
                        },
                        new Article
                        {
                            Caption = "C"
                        }
                    }
                },
                new Blog
                {
                    Title = "Y"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Blog>();
                dbContext.Blogs.AddRange(blogs);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/blogs?include=articles.revisions&sort=title&sort[articles]=caption,url&sort[articles.revisions]=-publishTime";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blogs[0].StringId);

            responseDocument.Included.Should().HaveCount(7);

            responseDocument.Included[0].Type.Should().Be("articles");
            responseDocument.Included[0].Id.Should().Be(blogs[0].Articles[2].StringId);

            responseDocument.Included[1].Type.Should().Be("articles");
            responseDocument.Included[1].Id.Should().Be(blogs[0].Articles[1].StringId);

            responseDocument.Included[2].Type.Should().Be("articles");
            responseDocument.Included[2].Id.Should().Be(blogs[0].Articles[0].StringId);

            responseDocument.Included[3].Type.Should().Be("revisions");
            responseDocument.Included[3].Id.Should().Be(blogs[0].Articles[0].Revisions.Skip(2).First().StringId);

            responseDocument.Included[4].Type.Should().Be("revisions");
            responseDocument.Included[4].Id.Should().Be(blogs[0].Articles[0].Revisions.Skip(0).First().StringId);

            responseDocument.Included[5].Type.Should().Be("revisions");
            responseDocument.Included[5].Id.Should().Be(blogs[0].Articles[0].Revisions.Skip(1).First().StringId);
            
            responseDocument.Included[6].Type.Should().Be("articles");
            responseDocument.Included[6].Id.Should().Be(blogs[0].Articles[3].StringId);
        }

        [Fact]
        public async Task Can_sort_on_HasOne_relationship()
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

            var route = "/api/v1/articles?sort=-author.lastName";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(articles[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(articles[0].StringId);
        }

        [Fact]
        public async Task Can_sort_in_multiple_scopes()
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
                        "sort=-title&" +
                        "sort[owner.articles]=-caption&" +
                        "sort[owner.articles.revisions]=-publishTime";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(blogs[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(blogs[0].StringId);

            responseDocument.Included.Should().HaveCount(5);
            responseDocument.Included[0].Id.Should().Be(blogs[1].Owner.StringId);
            responseDocument.Included[1].Id.Should().Be(blogs[1].Owner.Articles[1].StringId);
            responseDocument.Included[2].Id.Should().Be(blogs[1].Owner.Articles[1].Revisions.Skip(1).First().StringId);
            responseDocument.Included[3].Id.Should().Be(blogs[1].Owner.Articles[1].Revisions.Skip(0).First().StringId);
            responseDocument.Included[4].Id.Should().Be(blogs[1].Owner.Articles[0].StringId);
        }

        [Fact]
        public async Task Cannot_sort_in_unknown_scope()
        {
            // Arrange
            var route = "/api/v1/people?sort[doesNotExist]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource 'people'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort[doesNotExist]");
        }

        [Fact]
        public async Task Cannot_sort_in_unknown_nested_scope()
        {
            // Arrange
            var route = "/api/v1/people?sort[todoItems.doesNotExist]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("The specified sort is invalid.");
            responseDocument.Errors[0].Detail.Should().Be("Relationship 'doesNotExist' in 'todoItems.doesNotExist' does not exist on resource 'todoItems'.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort[todoItems.doesNotExist]");
        }

        [Fact]
        public async Task Cannot_sort_on_attribute_with_blocked_capability()
        {
            // Arrange
            var route = "/api/v1/todoItems?sort=achievedDate";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            
            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Sorting on the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().Be("Sorting on attribute 'achievedDate' is not allowed.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("sort");
        }

        [Fact]
        public async Task Can_sort_descending_by_ID()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person {Id = 3, LastName = "B"},
                new Person {Id = 2, LastName = "A"},
                new Person {Id = 1, LastName = "A"}
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Person>();
                dbContext.People.AddRange(persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/people?sort=lastName,-id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[0].StringId);
        }

        [Fact]
        public async Task Sorts_by_ID_if_none_specified()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person {Id = 3},
                new Person {Id = 2},
                new Person {Id = 1},
                new Person {Id = 4}
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Person>();
                dbContext.People.AddRange(persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/people";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(4);
            responseDocument.ManyData[0].Id.Should().Be(persons[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(persons[1].StringId);
            responseDocument.ManyData[2].Id.Should().Be(persons[0].StringId);
            responseDocument.ManyData[3].Id.Should().Be(persons[3].StringId);
        }
    }
}
