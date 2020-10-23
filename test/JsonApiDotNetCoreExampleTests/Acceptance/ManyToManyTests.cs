using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class ManyToManyTests : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        private readonly Faker<Author> _authorFaker;
        private readonly Faker<Article> _articleFaker;
        private readonly Faker<Tag> _tagFaker;

        public ManyToManyTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            _authorFaker = new Faker<Author>()
                .RuleFor(a => a.LastName, f => f.Random.Words(2));

            _articleFaker = new Faker<Article>()
                .RuleFor(a => a.Caption, f => f.Random.AlphaNumeric(10))
                .RuleFor(a => a.Author, f => _authorFaker.Generate());

            _tagFaker = new Faker<Tag>()
                .CustomInstantiator(f => new Tag())
                .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));
        }

        [Fact]
        public async Task Can_Get_HasManyThrough_Relationship_Through_Secondary_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ArticleTags.Add(existingArticleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Id.Should().Be(existingArticleTag.Tag.StringId);
            responseDocument.ManyData[0].Attributes["name"].Should().Be(existingArticleTag.Tag.Name);
        }

        [Fact]
        public async Task Can_Get_HasManyThrough_Through_Relationship_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ArticleTags.Add(existingArticleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Id.Should().Be(existingArticleTag.Tag.StringId);
            responseDocument.ManyData[0].Attributes.Should().BeNull();
        }

        [Fact]
        public async Task Can_Set_HasManyThrough_Relationship_Through_Primary_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            var existingTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingArticleTag, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    id = existingArticleTag.Article.StringId,
                    relationships = new
                    {
                        tags = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "tags",
                                    id = existingTag.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticleTag.Article.Id);

                articleInDatabase.ArticleTags.Should().HaveCount(1);
                articleInDatabase.ArticleTags.Single().TagId.Should().Be(existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_Set_With_Overlap_To_HasManyThrough_Relationship_Through_Primary_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            var existingTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingArticleTag, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    id = existingArticleTag.Article.StringId,
                    relationships = new
                    {
                        tags = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "tags",
                                    id = existingArticleTag.Tag.StringId
                                },
                                new
                                {
                                    type = "tags",
                                    id = existingTag.StringId
                                }
                            }
                        }
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticleTag.Article.Id);

                articleInDatabase.ArticleTags.Should().HaveCount(2);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingArticleTag.Tag.Id);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_Set_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            var existingTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingArticleTag, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = existingTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticleTag.Article.Id);

                articleInDatabase.ArticleTags.Should().HaveCount(1);
                articleInDatabase.ArticleTags.Single().TagId.Should().Be(existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_Add_To_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            var existingTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingArticleTag, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = existingTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticleTag.Article.Id);

                articleInDatabase.ArticleTags.Should().HaveCount(2);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingArticleTag.Tag.Id);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_Add_Already_Related_Resource_Without_It_Being_Readded_To_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var existingArticle = _articleFaker.Generate();
            existingArticle.ArticleTags = new HashSet<ArticleTag>
            {
                new ArticleTag {Tag = _tagFaker.Generate()},
                new ArticleTag {Tag = _tagFaker.Generate()}
            };

            var existingTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingArticle, existingTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = existingArticle.ArticleTags.ElementAt(1).Tag.StringId
                    },
                    new
                    {
                        type = "tags",
                        id = existingTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticle.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticle.Id);

                articleInDatabase.ArticleTags.Should().HaveCount(3);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingArticle.ArticleTags.ElementAt(0).Tag.Id);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingArticle.ArticleTags.ElementAt(1).Tag.Id);
                articleInDatabase.ArticleTags.Should().ContainSingle(articleTag => articleTag.TagId == existingTag.Id);
            });
        }

        [Fact]
        public async Task Can_Delete_From_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var existingArticleTag = new ArticleTag
            {
                Article = _articleFaker.Generate(),
                Tag = _tagFaker.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ArticleTags.AddRange(existingArticleTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = existingArticleTag.Tag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{existingArticleTag.Article.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var articleInDatabase = await dbContext.Articles
                    .Include(article => article.ArticleTags)
                    .FirstAsync(article => article.Id == existingArticleTag.Article.Id);

                articleInDatabase.ArticleTags.Should().BeEmpty();
            });
        }
    }
}
