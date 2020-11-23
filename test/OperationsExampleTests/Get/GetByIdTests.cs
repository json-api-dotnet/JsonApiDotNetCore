using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Internal;
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
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(data.Operations);
            Assert.Equal(author.Id.ToString(), data.Operations.Single().DataObject.Id);
        }

        [Fact]
        public async Task Get_Author_By_Id_Returns_404_If_NotFound()
        {
            // arrange
            var authorId = _faker.Random.Int(max: 0).ToString();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "get"},
                        { "ref",  new { type = "authors", id = authorId } }
                    }
                }
            };

            // act
            var (response, data) = await PatchAsync<ErrorCollection>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(data);
            Assert.Single(data.Errors);
            Assert.True(data.Errors[0].Detail.Contains("authors"), "The error detail should contain the name of the entity that could not be found.");
            Assert.True(data.Errors[0].Detail.Contains(authorId), "The error detail should contain the entity id that could not be found");
            Assert.True(data.Errors[0].Title.Contains("operation[0]"), "The error title should contain the operation identifier that failed");
        }
    }
}
