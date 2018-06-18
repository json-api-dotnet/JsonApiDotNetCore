using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    public class AddTests : Fixture
    {
        private readonly Faker _faker = new Faker();

        [Fact]
        public async Task Can_Create_Author()
        {
            // arrange
            var context = GetService<AppDbContext>();
            var author = AuthorFactory.Get();
            var content = new
            {
                operations = new[] {
                    new {
                        op = "add",
                        data = new {
                            type = "authors",
                            attributes = new {
                                name = author.Name
                            }
                        }
                    }
                }
            };

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var id = data.Operations.Single().DataObject.Id;
            var lastAuthor = await context.Authors.SingleAsync(a => a.StringId == id);
            Assert.Equal(author.Name, lastAuthor.Name);
        }

        [Fact]
        public async Task Can_Create_Authors()
        {
            // arrange
            var expectedCount = _faker.Random.Int(1, 10);
            var context = GetService<AppDbContext>();
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
                                 name = authors[i].Name
                             }
                         }
                     }
                );
            }

            // act
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedCount, data.Operations.Count);

            for (int i = 0; i < expectedCount; i++)
            {
                var dataObject = data.Operations[i].DataObject;
                var author = context.Authors.Single(a => a.StringId == dataObject.Id);
                Assert.Equal(authors[i].Name, author.Name);
            }
        }

        [Fact]
        public async Task Can_Create_Author_With_Article()
        {
            // arrange
            var context = GetService<AppDbContext>();
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
                                name = author.Name
                            },
                        }
                    },
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                name = article.Name
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
            var (response, data) = await PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, data.Operations.Count);

            var authorOperationResult = data.Operations[0];
            var id = authorOperationResult.DataObject.Id;
            var lastAuthor = await context.Authors
                .Include(a => a.Articles)
                .SingleAsync(a => a.StringId == id);
            var articleOperationResult = data.Operations[1];

            // author validation
            Assert.Equal(authorLocalId, authorOperationResult.DataObject.LocalId);
            Assert.Equal(author.Name, lastAuthor.Name);

            // article validation
            Assert.Single(lastAuthor.Articles);
            Assert.Equal(article.Name, lastAuthor.Articles[0].Name);
            Assert.Equal(articleOperationResult.DataObject.Id, lastAuthor.Articles[0].StringId);
        }
    }
}
