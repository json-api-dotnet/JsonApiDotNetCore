using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            
            FakeLoggerFactory loggerFactory = null;

            testContext.ConfigureLogging(options =>
            {
                loggerFactory = new FakeLoggerFactory();

                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddFilter((category, level) => level == LogLevel.Trace &&
                    (category == typeof(JsonApiReader).FullName || category == typeof(JsonApiWriter).FullName));
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                if (loggerFactory != null)
                {
                    services.AddSingleton(_ => loggerFactory);
                }
            });
        }
        
        [Fact]
        public async Task Can_Get_HasManyThrough_Relationship_Through_Secondary_Endpoint()
        {
            // Arrange
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(article, tag, articleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.Id}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.ManyData.Should().ContainSingle();
            responseDocument.ManyData[0].Id.Should().Be(tag.StringId);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Attributes["name"].Should().Be(tag.Name);
        }
        
        [Fact]
        public async Task Can_Get_HasManyThrough_Through_Relationship_Endpoint()
        {
            // Arrange
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(article, tag, articleTag);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";
            
            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            responseDocument.ManyData.Should().ContainSingle();
            responseDocument.ManyData[0].Id.Should().Be(tag.StringId);
            responseDocument.ManyData[0].Type.Should().Be("tags");
            responseDocument.ManyData[0].Attributes.Should().BeNull();
        }
        
        [Fact]
        public async Task Can_Create_Resource_With_HasManyThrough_Relationship()
        {
            // Arrange
            var tag = _tagFaker.Generate();
            var author = _authorFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(tag, author);
                await dbContext.SaveChangesAsync();
            });
            
            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        {"caption", "An article with relationships"}
                    },
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "author",  new {
                            data = new
                            {
                                type = "authors",
                                id = author.StringId
                            }
                        } },
                        {  "tags", new {
                            data = new dynamic[]
                            {
                                new {
                                    type = "tags",
                                    id = tag.StringId
                                }
                            }
                        } }
                    }
                }
            };
            
            var route = $"/api/v1/articles";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == int.Parse(responseDocument.SingleData.Id));

                persistedArticle.ArticleTags.Should().ContainSingle();
                persistedArticle.ArticleTags.First().TagId.Should().Be(tag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Set_HasManyThrough_Relationship_Through_Primary_Endpoint()
        {
            // Arrange
            var firstTag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = firstTag
            };
            var secondTag = _tagFaker.Generate();
    
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(firstTag, secondTag, article, articleTag);
                await dbContext.SaveChangesAsync();
            });
            
            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "tags",  new {
                            data = new [] { new
                            {
                                type = "tags",
                                id = secondTag.StringId
                            }  }
                        } }
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().ContainSingle();
                persistedArticle.ArticleTags.First().TagId.Should().Be(secondTag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Set_With_Overlap_To_HasManyThrough_Relationship_Through_Primary_Endpoint()
        {
            // Arrange
            var firstTag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = firstTag
            };
            var secondTag = _tagFaker.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(firstTag, secondTag, article, articleTag);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    id = article.StringId,
                    relationships = new Dictionary<string, dynamic>
                    {
                        {  "tags",  new {
                            data = new [] { new
                            {
                                type = "tags",
                                id = firstTag.StringId
                            },   new
                            {
                                type = "tags",
                                id = secondTag.StringId
                            }  }
                        } }
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}";


            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);


            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().HaveCount(2);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == firstTag.Id);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == secondTag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Set_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var tag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            var secondTag = _tagFaker.Generate();
    
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(tag, secondTag, article, articleTag);
                await dbContext.SaveChangesAsync();
            });
            
            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = secondTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().ContainSingle();
                persistedArticle.ArticleTags.First().TagId.Should().Be(secondTag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Add_To_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var firstTag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = firstTag
            };
            var secondTag = _tagFaker.Generate();
    
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(firstTag, secondTag, article, articleTag);
                await dbContext.SaveChangesAsync();
            });
    
            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = secondTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().HaveCount(2);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == firstTag.Id);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == secondTag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Add_Already_Related_Resource_Without_It_Being_Readded_To_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var article = _articleFaker.Generate();
            var firstTag = _tagFaker.Generate();
            var secondTag = _tagFaker.Generate();
            article.ArticleTags = new HashSet<ArticleTag> {new ArticleTag  {Article = article, Tag = firstTag}, new ArticleTag  {Article = article, Tag = secondTag} };

            var thirdTag = _tagFaker.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(firstTag, secondTag, article, thirdTag);
                await dbContext.SaveChangesAsync();
            });
            
            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = secondTag.StringId
                    },
                    new
                    {
                        type = "tags",
                        id = thirdTag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";

            // Act
            var (httpResponse, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().HaveCount(3);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == firstTag.Id);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == secondTag.Id);
                persistedArticle.ArticleTags.Should().ContainSingle(at => at.TagId == thirdTag.Id);
            });
        }
        
        [Fact]
        public async Task Can_Delete_From_HasManyThrough_Relationship_Through_Relationships_Endpoint()
        {
            // Arrange
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };

            article.ArticleTags = new HashSet<ArticleTag> {articleTag};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(tag, article, articleTag);
                await dbContext.SaveChangesAsync();
            });
            
            var requestBody = new
            {
                data = new[]
                {
                    new
                    {
                        type = "tags",
                        id = tag.StringId
                    }
                }
            };

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";

            // Act
            var (httpResponse, _) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var persistedArticle = await dbContext.Articles
                    .Include(a => a.ArticleTags)
                    .FirstAsync(a => a.Id == article.Id);

                persistedArticle.ArticleTags.Should().BeEmpty();
            });
        }
    }
}
