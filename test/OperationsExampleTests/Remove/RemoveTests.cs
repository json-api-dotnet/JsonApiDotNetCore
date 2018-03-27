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
    public class RemoveTests : Fixture
    {
        private readonly Faker _faker = new Faker();

        [Fact]
        public async Task Can_Remove_Author()
        {
            // arrange
            var context = GetService<AppDbContext>();
            var author = AuthorFactory.Get();
            context.Authors.Add(author);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "remove"},
                        { "ref",  new { type = "authors", id = author.StringId } }
                    }
                }
            };

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(data.Operations);
            Assert.Null(context.Authors.SingleOrDefault(a => a.Id == author.Id));
        }

        [Fact]
        public async Task Can_Remove_Authors()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);
            var context = GetService<AppDbContext>();

            var authors = AuthorFactory.Get(count);

            context.Authors.AddRange(authors);
            context.SaveChanges();

            var content = new
            {
                operations = new List<object>()
            };

            for (int i = 0; i < count; i++)
                content.operations.Add(
                    new Dictionary<string, object> {
                        { "op", "remove"},
                        { "ref",  new { type = "authors", id = authors[i].StringId } }
                    }
                );

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(data.Operations);

            for (int i = 0; i < count; i++)
                Assert.Null(context.Authors.SingleOrDefault(a => a.Id == authors[i].Id));
        }
    }
}
