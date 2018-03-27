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
    public class GetTests : Fixture, IDisposable
    {
        private readonly Faker _faker = new Faker();

        [Fact]
        public async Task Can_Get_Authors()
        {
            // arrange
            var expectedCount = _faker.Random.Int(1, 10);
            var context = GetService<AppDbContext>();
            context.Articles.RemoveRange(context.Articles);
            context.Authors.RemoveRange(context.Authors);
            var authors = AuthorFactory.Get(expectedCount);
            context.AddRange(authors);
            context.SaveChanges();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "get"},
                        { "ref",  new { type = "authors" } }
                    }
                }
            };

            // act
            var result = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(1, result.data.Operations.Count);
            Assert.Equal(expectedCount, result.data.Operations.Single().DataList.Count);
        }

        [Fact]
        public async Task Can_Get_Author_By_Id()
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
                        { "op", "get"},
                        { "ref",  new { type = "authors", id = author.StringId } }
                    }
                }
            };

            // act
            var result = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(1, result.data.Operations.Count);
            Assert.Equal(author.Id.ToString(), result.data.Operations.Single().DataObject.Id);
        }
    }
}
