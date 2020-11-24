using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using OperationsExample;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests.Add
{
    [Collection("WebHostCollection")]
    public class AddTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker _faker = new Faker();

        public AddTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Create_Author()
        {
            // arrange
            var author = AuthorFactory.Get();
            var content = new
            {
                operations = new[] {
                    new {
                        op = "add",
                        data = new {
                            type = "authors",
                            attributes = new {
                                firstName = author.FirstName
                            }
                        }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);

            var id = int.Parse(data.Operations.Single().SingleData.Id);
            var lastAuthor = await _fixture.Context.AuthorDifferentDbContextName.SingleAsync(a => a.Id == id);
            Assert.Equal(author.FirstName, lastAuthor.FirstName);
        }

        [Fact]
        public async Task Can_Create_Authors()
        {
            // arrange
            var expectedCount = _faker.Random.Int(1, 10);
            var authors = AuthorFactory.Get(expectedCount);
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
                             type = "authors",
                             attributes = new
                             {
                                 firstName = authors[i].FirstName
                             }
                         }
                     }
                );
            }

            // act
            var (response, data) = await _fixture.PatchAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Equal(expectedCount, data.Operations.Count);

            for (int i = 0; i < expectedCount; i++)
            {
                var dataObject = data.Operations[i].SingleData;
                var author = _fixture.Context.AuthorDifferentDbContextName.Single(a => a.Id == int.Parse(dataObject.Id));
                Assert.Equal(authors[i].FirstName, author.FirstName);
            }
        }

        [Fact]
        public async Task Can_Create_Article_With_Existing_Author()
        {
            // arrange
            var context = _fixture.Context;
            var author = AuthorFactory.Get();
            var article = ArticleFactory.Get();

            context.AuthorDifferentDbContextName.Add(author);
            await context.SaveChangesAsync();


            //const string authorLocalId = "author-1";

            var content = new
            {
                operations = new object[] {
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                caption = article.Caption
                            },
                            relationships = new {
                                author = new {
                                    data = new {
                                        type = "authors",
                                        id = author.Id
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Single(data.Operations);


            var lastAuthor = await context.AuthorDifferentDbContextName
                .Include(a => a.Articles)
                .SingleAsync(a => a.Id == author.Id);
            var articleOperationResult = data.Operations[0];

            // author validation: sanity checks
            Assert.NotNull(lastAuthor);
            Assert.Equal(author.FirstName, lastAuthor.FirstName);

            //// article validation
            Assert.Single(lastAuthor.Articles);
            Assert.Equal(article.Caption, lastAuthor.Articles[0].Caption);
            Assert.Equal(articleOperationResult.SingleData.Id, lastAuthor.Articles[0].StringId);
        }

        [Fact]
        public async Task Can_Create_Articles_With_Existing_Author()
        {
            // arrange
            var author = AuthorFactory.Get();
            _fixture.Context.AuthorDifferentDbContextName.Add(author);
            await _fixture.Context.SaveChangesAsync();
            var expectedCount = _faker.Random.Int(1, 10);
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
                                 caption = articles[i].Caption
                             },
                             relationships = new
                             {
                                 author = new
                                 {
                                     data = new
                                     {
                                         type = "authors",
                                         id = author.Id
                                     }
                                 }
                             }
                         }
                     }
                );
            }

            // act
            var (response, data) = await _fixture.PatchAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Equal(expectedCount, data.Operations.Count);

            // author validation: sanity checks
            var lastAuthor = _fixture.Context.AuthorDifferentDbContextName.Include(a => a.Articles).Single(a => a.Id == author.Id);
            Assert.NotNull(lastAuthor);
            Assert.Equal(author.FirstName, lastAuthor.FirstName);

            // articles validation
            Assert.True(lastAuthor.Articles.Count == expectedCount);
            for (int i = 0; i < expectedCount; i++)
            {
                var article = articles[i];
                Assert.NotNull(lastAuthor.Articles.FirstOrDefault(a => a.Caption == article.Caption));
            }
        }

        [Fact]
        public async Task Can_Create_Author_With_Article_Using_LocalId()
        {
            // arrange
            var author = AuthorFactory.Get();
            var article = ArticleFactory.Get();
            const string authorLocalId = "author-1";

            var content = new
            {
                operations = new object[] {
                    new {
                        op = "add",
                        data = new {
                            lid = authorLocalId,
                            type = "authors",
                            attributes = new {
                                firstName = author.FirstName
                            },
                        }
                    },
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                caption = article.Caption
                            },
                            relationships = new {
                                author = new {
                                    data = new {
                                        type = "authors",
                                        lid = authorLocalId
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<AtomicOperationsDocument>("api/v1/operations", content);

            // assert
            Assert.NotNull(response);
            _fixture.AssertEqualStatusCode(HttpStatusCode.OK, response);
            Assert.Equal(2, data.Operations.Count);

            var authorOperationResult = data.Operations[0];
            var id = int.Parse(authorOperationResult.SingleData.Id);
            var lastAuthor = await _fixture.Context.AuthorDifferentDbContextName
                .Include(a => a.Articles)
                .SingleAsync(a => a.Id == id);
            var articleOperationResult = data.Operations[1];

            // author validation
            Assert.Equal(authorLocalId, authorOperationResult.SingleData.LocalId);
            Assert.Equal(author.FirstName, lastAuthor.FirstName);

            // article validation
            Assert.Single(lastAuthor.Articles);
            Assert.Equal(article.Caption, lastAuthor.Articles[0].Caption);
            Assert.Equal(articleOperationResult.SingleData.Id, lastAuthor.Articles[0].StringId);
        }
    }
}
