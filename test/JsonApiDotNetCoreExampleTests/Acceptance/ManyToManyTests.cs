using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class ManyToManyTests
    {
        private static readonly Faker<Article> _articleFaker = new Faker<Article>()
            .RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10))
            .RuleFor(a => a.Author, f => new Author());

        private static readonly Faker<Tag> _tagFaker = new Faker<Tag>().RuleFor(a => a.Name, f => f.Random.AlphaNumeric(10));

        private TestFixture<TestStartup> _fixture;
        public ManyToManyTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Fetch_Many_To_Many_Through_All()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();

            context.Articles.RemoveRange(context.Articles);
            await context.SaveChangesAsync();

            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            context.ArticleTags.Add(articleTag);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles?include=tags";

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var document = JsonConvert.DeserializeObject<Documents>(body);
            Assert.NotEmpty(document.Included);

            var articleResponseList = _fixture.GetService<IJsonApiDeserializer>().DeserializeList<Article>(body);
            Assert.NotNull(articleResponseList);
            
            var articleResponse = articleResponseList.FirstOrDefault(a => a.Id == article.Id);
            Assert.NotNull(articleResponse);
            Assert.Equal(article.Name, articleResponse.Name);

            var tagResponse = Assert.Single(articleResponse.Tags);
            Assert.Equal(tag.Id, tagResponse.Id);
            Assert.Equal(tag.Name, tagResponse.Name);
        }

        [Fact]
        public async Task Can_Fetch_Many_To_Many_Through_GetById()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            context.ArticleTags.Add(articleTag);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}?include=tags";

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");
            
            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.NotEmpty(document.Included);

            var articleResponse = _fixture.GetService<IJsonApiDeserializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);
            Assert.Equal(article.Id, articleResponse.Id);

            var tagResponse = Assert.Single(articleResponse.Tags);
            Assert.Equal(tag.Id, tagResponse.Id);
            Assert.Equal(tag.Name, tagResponse.Name);
        }

        [Fact]
        public async Task Can_Fetch_Many_To_Many_Without_Include()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = tag
            };
            context.ArticleTags.Add(articleTag);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";

            // act
            var response = await _fixture.Client.GetAsync(route);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var document = JsonConvert.DeserializeObject<Document>(body);
            Assert.Null(document.Data.Relationships["tags"].ManyData);
        }

        [Fact]
        public async Task Can_Create_Many_To_Many()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var tag = _tagFaker.Generate();
            var author = new Author();
            context.Tags.Add(tag);
            context.Authors.Add(author);
            await context.SaveChangesAsync();

            var article = _articleFaker.Generate();

            var route = "/api/v1/articles";
            var request = new HttpRequestMessage(new HttpMethod("POST"), route);
            var content = new
            {
                data = new
                {
                    type = "articles",
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

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.Created == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var articleResponse = _fixture.GetService<IJsonApiDeserializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);
            
            var persistedArticle = await _fixture.Context.Articles
                .Include(a => a.ArticleTags)
                .SingleAsync(a => a.Id == articleResponse.Id);

            var persistedArticleTag = Assert.Single(persistedArticle.ArticleTags);
            Assert.Equal(tag.Id, persistedArticleTag.TagId);
        }

        [Fact]
        public async Task Can_Update_Many_To_Many()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var tag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            context.Tags.Add(tag);
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), route);
            var content = new
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
                                id = tag.StringId
                            } }
                        } }
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");
            
            var articleResponse = _fixture.GetService<IJsonApiDeserializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);
            
            _fixture.ReloadDbContext();
            var persistedArticle = await _fixture.Context.Articles
                .Include(a => a.ArticleTags)
                .SingleAsync(a => a.Id == articleResponse.Id);

            var persistedArticleTag = Assert.Single(persistedArticle.ArticleTags);
            Assert.Equal(tag.Id, persistedArticleTag.TagId);
        }

        [Fact]
        public async Task Can_Update_Many_To_Many_With_Complete_Replacement()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var firstTag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = firstTag
            };
            context.ArticleTags.Add(articleTag);
            var secondTag = _tagFaker.Generate();
            context.Tags.Add(secondTag);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), route);
            var content = new
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

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var articleResponse = _fixture.GetService<IJsonApiDeserializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);

            _fixture.ReloadDbContext();
            var persistedArticle = await _fixture.Context.Articles
                .Include("ArticleTags.Tag")
                .SingleOrDefaultAsync(a => a.Id == article.Id);
            var tag = persistedArticle.ArticleTags.Select(at => at.Tag).Single();
            Assert.Equal(secondTag.Id, tag.Id);
        }

        [Fact]
        public async Task Can_Update_Many_To_Many_With_Complete_Replacement_With_Overlap()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var firstTag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            var articleTag = new ArticleTag
            {
                Article = article,
                Tag = firstTag
            };
            context.ArticleTags.Add(articleTag);
            var secondTag = _tagFaker.Generate();
            context.Tags.Add(secondTag);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), route);
            var content = new
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

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");

            var articleResponse = _fixture.GetService<IJsonApiDeserializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);

            _fixture.ReloadDbContext();
            var persistedArticle = await _fixture.Context.Articles
                .Include(a => a.ArticleTags)
                .SingleOrDefaultAsync( a => a.Id == article.Id);
            var tags = persistedArticle.ArticleTags.Select(at => at.Tag).ToList();
            Assert.Equal(2, tags.Count);
        }

        [Fact]
        public async Task Can_Update_Many_To_Many_Through_Relationship_Link()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var tag = _tagFaker.Generate();
            var article = _articleFaker.Generate();
            context.Tags.Add(tag);
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var route = $"/api/v1/articles/{article.Id}/relationships/tags";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), route);
            var content = new
            {
                data = new [] { 
                    new {
                        type = "tags",
                        id = tag.StringId
                    } 
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // act
            var response = await _fixture.Client.SendAsync(request);

            // assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{route} returned {response.StatusCode} status code with payload: {body}");
            
            _fixture.ReloadDbContext();
            var persistedArticle = await _fixture.Context.Articles
                .Include(a => a.ArticleTags)
                .SingleAsync(a => a.Id == article.Id);

            var persistedArticleTag = Assert.Single(persistedArticle.ArticleTags);
            Assert.Equal(tag.Id, persistedArticleTag.TagId);
        }
    }
}
