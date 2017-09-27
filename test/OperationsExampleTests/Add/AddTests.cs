using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using Microsoft.EntityFrameworkCore;
using OperationsExample.Data;
using OperationsExampleTests.Factories;
using Xunit;

namespace OperationsExampleTests
{
    [Collection("WebHostCollection")]
    public class AddTests
    {
        private readonly Fixture _fixture;

        public AddTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Create_Article()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var article = ArticleFactory.Get();
            var content = new
            {
                operations = new[] {
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                name = article.Name
                            }
                        }
                    }
                }
            };

            // act
            var response = await _fixture.PatchAsync("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var lastArticle = await context.Articles.LastAsync();
            Assert.Equal(article.Name, lastArticle.Name);
        }

        [Fact]
        public async Task Can_Create_Articles()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var articles = ArticleFactory.Get(2);
            var content = new
            {
                operations = new[] {
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                name = articles[0].Name
                            }
                        }
                    },
                    new {
                        op = "add",
                        data = new {
                            type = "articles",
                            attributes = new {
                                name = articles[1].Name
                            }
                        }
                    }
                }
            };

            // act
            var response = await _fixture.PatchAsync("api/bulk", content);

            // assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var lastArticles = (await context.Articles
                .OrderByDescending(d => d.Id)
                .Take(2)
                .ToListAsync())
                .OrderBy(l => l.Id)
                .ToList();

            Assert.Equal(articles[0].Name, lastArticles[0].Name);
            Assert.Equal(articles[1].Name, lastArticles[1].Name);
        }
    }
}
