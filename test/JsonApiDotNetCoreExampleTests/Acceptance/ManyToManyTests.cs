using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Person = JsonApiDotNetCoreExample.Models.Person;

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
        public async Task Can_Fetch_Many_To_Many_Through()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = _articleFaker.Generate();
            var tag = _tagFaker.Generate();
            var articleTag = new ArticleTag { 
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
            
            var articleResponse = _fixture.GetService<IJsonApiDeSerializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);
            Assert.Equal(article.Id, articleResponse.Id);
            
            var tagResponse = Assert.Single(articleResponse.Tags);
            Assert.Equal(tag.Id, tagResponse.Id);
        }

        [Fact]
        public async Task Can_Create_Many_To_Many()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var tag = _tagFaker.Generate();
            var author = new Person();
            context.Tags.Add(tag);
            context.People.Add(author);
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
                                type = "people",
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
            
            var articleResponse = _fixture.GetService<IJsonApiDeSerializer>().Deserialize<Article>(body);
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
            
            var articleResponse = _fixture.GetService<IJsonApiDeSerializer>().Deserialize<Article>(body);
            Assert.NotNull(articleResponse);
            
            _fixture.ReloadDbContext();
            var persistedArticle = await _fixture.Context.Articles
                .Include(a => a.ArticleTags)
                .SingleAsync(a => a.Id == articleResponse.Id);

            var persistedArticleTag = Assert.Single(persistedArticle.ArticleTags);
            Assert.Equal(tag.Id, persistedArticleTag.TagId);
        }
    }
}