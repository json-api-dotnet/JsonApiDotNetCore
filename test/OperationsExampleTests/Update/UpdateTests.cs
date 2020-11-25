using Bogus;
using OperationsExampleTests.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;
using OperationsExample;
using Xunit;

namespace OperationsExampleTests.Update
{
    [Collection("WebHostCollection")]
    public class UpdateTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker _faker = new Faker();

        public UpdateTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Update_Author()
        {
            // arrange
            var author = AuthorFactory.Get();
            var updates = AuthorFactory.Get();
            _fixture.Context.AuthorDifferentDbContextName.Add(author);
            _fixture.Context.SaveChanges();

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
                                firstName = updates.FirstName
                            }
                        } },
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PostAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.NotNull(data);
            Assert.Single(data.Operations);

            Assert.Equal(AtomicOperationCode.Update, data.Operations.Single().Code);
            Assert.Null(data.Operations.Single().SingleData);
        }

        [Fact]
        public async Task Can_Update_Authors()
        {
            // arrange
            var count = _faker.Random.Int(1, 10);

            var authors = AuthorFactory.Get(count);
            var updates = AuthorFactory.Get(count);

            _fixture.Context.AuthorDifferentDbContextName.AddRange(authors);
            _fixture.Context.SaveChanges();

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
                            firstName = updates[i].FirstName
                        }
                    } },
                });

            // act
            var (response, data) = await _fixture.PostAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.NotNull(data);
            Assert.Equal(count, data.Operations.Count);

            for (int i = 0; i < count; i++)
            {
                Assert.Equal(AtomicOperationCode.Update, data.Operations[i].Code);
                Assert.Null(data.Operations[i].SingleData);
            }
        }
    }
}
