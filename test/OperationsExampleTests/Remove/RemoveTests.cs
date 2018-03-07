using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models.Operations;
using OperationsExample.Data;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    [Collection("WebHostCollection")]
    public class RemoveTests
    {
        private readonly Fixture _fixture;
        private readonly Faker _faker = new Faker();

        public RemoveTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Remove_Article()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = ArticleFactory.Get();
            context.Articles.Add(article);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "remove"},
                        { "ref",  new { type = "articles", id = article.StringId } }
                    }
                }
            };

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(1, result.data.Operations.Count);
            Assert.Null(context.Articles.SingleOrDefault(a => a.Id == article.Id));
        }

        [Fact]
        public async Task Can_Remove_Articles()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);
            var context = _fixture.GetService<AppDbContext>();

            var articles = ArticleFactory.Get(count);

            context.Articles.AddRange(articles);
            context.SaveChanges();

            var content = new
            {
                operations = new List<object>()
            };

            for (int i = 0; i < count; i++)
                content.operations.Add(
                    new Dictionary<string, object> {
                        { "op", "remove"},
                        { "ref",  new { type = "articles", id = articles[i].StringId } }
                    }
                );

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(count, result.data.Operations.Count);

            for (int i = 0; i < count; i++)
                Assert.Null(context.Articles.SingleOrDefault(a => a.Id == articles[i].Id));
        }
    }
}
