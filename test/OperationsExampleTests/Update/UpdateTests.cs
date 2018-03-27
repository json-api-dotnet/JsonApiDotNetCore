using Bogus;
using JsonApiDotNetCore.Models.Operations;
using OperationsExample.Data;
using OperationsExampleTests.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace OperationsExampleTests.Update
{
    public class UpdateTests : Fixture
    {
        private readonly Faker _faker = new Faker();

        [Fact]
        public async Task Can_Update_Author()
        {
            // arrange
            var context = GetService<AppDbContext>();
            var author = AuthorFactory.Get();
            var updates = AuthorFactory.Get();
            context.Authors.Add(author);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "update" },
                        { "ref", new {
                            type = "authors",
                            id = author.Id,
                        } },
                        { "data", new {
                            type = "authors",
                            id = author.Id,
                            attributes = new
                            {
                                name = updates.Name
                            }
                        } },
                    }
                }
            };

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(1, data.Operations.Count);

            var attrs = data.Operations.Single().DataObject.Attributes;
            Assert.Equal(updates.Name, attrs["name"]);
        }

        [Fact]
        public async Task Can_Update_Authors()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);
            var context = GetService<AppDbContext>();

            var authors = AuthorFactory.Get(count);
            var updates = AuthorFactory.Get(count);

            context.Authors.AddRange(authors);
            context.SaveChanges();

            var content = new
            {
                operations = new List<object>()
            };

            for (int i = 0; i < count; i++)
                content.operations.Add(new Dictionary<string, object> {
                    { "op", "update" },
                    { "ref", new {
                        type = "authors",
                        id = authors[i].Id,
                    } },
                    { "data", new {
                        type = "authors",
                        id = authors[i].Id,
                        attributes = new
                        {
                            name = updates[i].Name
                        }
                    } },
                });

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(data);
            Assert.Equal(count, data.Operations.Count);

            for (int i = 0; i < count; i++)
            {
                var attrs = data.Operations[i].DataObject.Attributes;
                Assert.Equal(updates[i].Name, attrs["name"]);
            }
        }
    }
}
