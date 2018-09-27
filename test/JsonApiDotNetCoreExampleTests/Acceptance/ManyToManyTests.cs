using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
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
    }
}