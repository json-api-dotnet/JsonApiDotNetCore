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
    public class ReplaceTests
    {
        private readonly Fixture _fixture;
        private readonly Faker _faker = new Faker();

        public ReplaceTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Update_Article()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = ArticleFactory.Get();
            var updates = ArticleFactory.Get();
            context.Articles.Add(article);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new {
                        op = "replace",
                        data = new {
                            type = "articles",
                            id = article.Id,
                            attributes = new {
                                name = updates.Name
                            }
                        }
                    },
                }
            };

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(1, result.data.Operations.Count);

            var attrs = result.data.Operations.Single().DataObject.Attributes;
            Assert.Equal(updates.Name, attrs["name"]);
        }

        [Fact]
        public async Task Can_Update_Articles()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);
            var context = _fixture.GetService<AppDbContext>();

            var articles = ArticleFactory.Get(count);
            var updates = ArticleFactory.Get(count);

            context.Articles.AddRange(articles);
            context.SaveChanges();

            var content = new
            {
                operations = new List<object>()
            };

            for (int i = 0; i < count; i++)
                content.operations.Add(new
                {
                    op = "replace",
                    data = new
                    {
                        type = "articles",
                        id = articles[i].Id,
                        attributes = new
                        {
                            name = updates[i].Name
                        }
                    }
                });

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(count, result.data.Operations.Count);

            for (int i = 0; i < count; i++)
            {
                var attrs = result.data.Operations[i].DataObject.Attributes;
                Assert.Equal(updates[i].Name, attrs["name"]);
            }
        }
    }
}
