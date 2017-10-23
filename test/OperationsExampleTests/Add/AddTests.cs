using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models.Operations;
using Microsoft.EntityFrameworkCore;
using OperationsExample.Data;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    [Collection("WebHostCollection")]
    public class AddTests
    {
        private readonly Fixture _fixture;
        private readonly Faker _faker = new Faker();

        public AddTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Create_Article()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = ArticleFactory.Get();
            var content = new
            {
                operations = new[] {
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                name = article.Name
                            }
                        }
                    }
                }
            };

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);

            var id = (string)result.data.Operations.Single().DataObject.Id;
            var lastArticle = await context.Articles.SingleAsync(a => a.StringId == id);
            Assert.Equal(article.Name, lastArticle.Name);
        }

        [Fact]
        public async Task Can_Create_Articles()
        {
            // arrange
            var expectedCount = _faker.Random.Int(1, 10);
            var context = _fixture.GetService<AppDbContext>();
            var articles = ArticleFactory.Get(expectedCount);
            var content = new
            {
                operations = new List<object>()
            };

            for (int i = 0; i < expectedCount; i++)
            {
                content.operations.Add(
                     new
                     {
                         op = "add",
                         data = new
                         {
                             type = "articles",
                             attributes = new
                             {
                                 name = articles[i].Name
                             }
                         }
                     }
                );
            }

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(expectedCount, result.data.Operations.Count);

            for (int i = 0; i < expectedCount; i++)
            {
                var data = result.data.Operations[i].DataObject;
                var article = context.Articles.Single(a => a.StringId == data.Id.ToString());
                Assert.Equal(articles[i].Name, article.Name);
            }
        }
    }
}
