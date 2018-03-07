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
    public class GetTests
    {
        private readonly Fixture _fixture;
        private readonly Faker _faker = new Faker();

        public GetTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_Articles()
        {
            // arrange
            var expectedCount = _faker.Random.Int(1, 10);
            var context = _fixture.GetService<AppDbContext>();
            context.Articles.RemoveRange(context.Articles);
            var articles = ArticleFactory.Get(expectedCount);
            context.AddRange(articles);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "get"},
                        { "ref",  new { type = "articles" } }
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
            Assert.Equal(expectedCount, result.data.Operations.Single().DataList.Count);
        }

        [Fact]
        public async Task Can_Get_Article_By_Id()
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
                        { "op", "get"},
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
            Assert.Equal(article.Id.ToString(), result.data.Operations.Single().DataObject.Id);
        }
    }
}
