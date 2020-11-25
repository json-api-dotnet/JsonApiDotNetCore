using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization.Objects;
using OperationsExample;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests.Remove
{
    [Collection("WebHostCollection")]
    public class RemoveTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker _faker = new Faker();

        public RemoveTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Remove_Author()
        {
            // arrange
            var author = AuthorFactory.Get();
            _fixture.Context.AuthorDifferentDbContextName.Add(author);
            _fixture.Context.SaveChanges();

            var content = new
            {
                atomic__operations = new[]
                {
                    new Dictionary<string, object>
                    {
                        {"op", "remove"},
                        {"ref", new {type = "authors", id = author.StringId}}
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PostAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Single(data.Results);
            Assert.Null(_fixture.Context.AuthorDifferentDbContextName.SingleOrDefault(a => a.Id == author.Id));
        }

        [Fact]
        public async Task Can_Remove_Authors()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);
            var authors = AuthorFactory.Get(count);

            _fixture.Context.AuthorDifferentDbContextName.AddRange(authors);
            _fixture.Context.SaveChanges();

            var content = new
            {
                atomic__operations = new List<object>()
            };

            for (int i = 0; i < count; i++)
            {
                content.atomic__operations.Add(
                    new Dictionary<string, object>
                    {
                        {"op", "remove"},
                        {"ref", new {type = "authors", id = authors[i].StringId}}
                    }
                );
            }

            // act
            var (response, data) = await _fixture.PostAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            Assert.NotNull(data);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Equal(count, data.Results.Count);

            for (int i = 0; i < count; i++)
            {
                Assert.Null(data.Results[i].Data);
                Assert.Null(_fixture.Context.AuthorDifferentDbContextName.SingleOrDefault(a => a.Id == authors[i].Id));
            }
        }
    }
}
