using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCoreExample.Data;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    public class GetRelationshipTests : Fixture, IDisposable
    {
        private readonly Faker _faker = new Faker();

        [Fact]
        public async Task Can_Get_Article_Author()
        {
            // arrange
            var context = GetService<AppDbContext>();
            var author = AuthorFactory.Get();
            var article = ArticleFactory.Get();
            article.Author = author;
            context.Articles.Add(article);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "get"},
                        { "ref",  new { type = "articles", id = article.StringId, relationship = nameof(article.Author) } }
                    }
                }
            };

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, data.Operations.Count);
            var resourceObject = data.Operations.Single().DataObject;
            Assert.Equal(author.Id.ToString(), resourceObject.Id);
            Assert.Equal("authors", resourceObject.Type);
        }
    }
}
