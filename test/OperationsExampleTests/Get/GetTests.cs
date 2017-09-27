using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models.Operations;
using Microsoft.EntityFrameworkCore;
using OperationsExample.Data;
using Xunit;

namespace OperationsExampleTests
{
    [Collection("WebHostCollection")]
    public class GetTests
    {
        private readonly Fixture _fixture;

        public GetTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_Get_Articles()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var articles = await context.Articles.ToListAsync();

            var content = new
            {
                operations = new[] {
                    new Dictionary<string, object> {
                        { "op", "get"},
                        { "ref",  new { type = "articles" } }
                    }
                }
            };

            // act
            var result = await _fixture.PatchAsync<OperationsDocument>("api/bulk", content);

            // assert
            Assert.NotNull(result.response);
            Assert.NotNull(result.data);
            Assert.Equal(HttpStatusCode.OK, result.response.StatusCode);
            Assert.Equal(1, result.data.Operations.Count);
            Assert.Equal(articles.Count, result.data.Operations[0].DataList.Count);
        }
    }
}
