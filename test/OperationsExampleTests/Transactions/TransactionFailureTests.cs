using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;
using OperationsExample.Data;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    [Collection("WebHostCollection")]
    public class TransactionFailureTests
    {
        private readonly Fixture _fixture;
        private readonly Faker _faker = new Faker();

        public TransactionFailureTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Create_Author_If_Article_Creation_Fails()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var author = AuthorFactory.Get();
            var article = ArticleFactory.Get();

            // do this so that the name is random enough for db validations
            author.Name = Guid.NewGuid().ToString("N");
            article.Name = Guid.NewGuid().ToString("N");

            var content = new
            {
                operations = new object[] {
                    new {
                        op = "add",
                        data = new {
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
                            // by not including the author, the article creation will fail
                            // relationships = new {
                            //    author = new {
                            //        data = new {
                            //            type = "authors",
                            //            lid = authorLocalId
                            //        }
                            //    }
                            // }
                        }
                    }
                }
            };

            // act
            var (response, data) = await _fixture.PatchAsync<ErrorCollection>("api/bulk", content);

            // assert
            Assert.NotNull(response);
            // for now, it is up to application implementations to perform validation and 
            // provide the proper HTTP response code
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(1, data.Errors.Count);
            Assert.Contains("operation[1] (add)", data.Errors[0].Title);

            var dbAuthors = await context.Authors.Where(a => a.Name == author.Name).ToListAsync();
            var dbArticles = await context.Articles.Where(a => a.Name == article.Name).ToListAsync();
            Assert.Empty(dbAuthors);
            Assert.Empty(dbArticles);
        }
    }
}
