using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using OperationsExample;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests.Transactions
{
    [Collection("WebHostCollection")]
    public class TransactionFailureTests
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly Faker _faker = new Faker();

        public TransactionFailureTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Create_Author_If_Article_Creation_Fails()
        {
            // arrange
            var author = AuthorFactory.Get();
            var article = ArticleFactory.Get();

            // do this so that the name is random enough for db validations
            author.FirstName = Guid.NewGuid().ToString("N");
            article.Caption = Guid.NewGuid().ToString("N");

            var content = new
            {
                operations = new object[] {
                    new {
                        op = "add",
                        data = new {
                            type = "authors",
                            attributes = new {
                                firstName = author.FirstName
                            }
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
                                        id = 99999999
                                    }
                                }
                             }
                        }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<ErrorDocument>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            // for now, it is up to application implementations to perform validation and 
            // provide the proper HTTP response code
            _fixture.AssertEqualStatusCode(HttpStatusCode.InternalServerError, response);
            Assert.Single(data.Errors);
            Assert.Contains("operation[1] (add)", data.Errors[0].Detail);

            var dbAuthors = await _fixture.Context.AuthorDifferentDbContextName.Where(a => a.FirstName == author.FirstName).ToListAsync();
            var dbArticles = await _fixture.Context.Articles.Where(a => a.Caption == article.Caption).ToListAsync();
            Assert.Empty(dbAuthors);
            Assert.Empty(dbArticles);
        }
    }
}
