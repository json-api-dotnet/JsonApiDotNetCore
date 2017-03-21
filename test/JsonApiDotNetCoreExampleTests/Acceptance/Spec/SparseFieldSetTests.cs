using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Xunit;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCoreExample.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class SparseFieldSetTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private readonly AppDbContext _dbContext;

        public SparseFieldSetTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Can_Select_Sparse_Fieldsets()
        {
            // arrange
            var fields = new string[] { "Id", "Description" };
            var todoItem = new TodoItem {
                Description = "description",
                Ordinal = 1
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();
            var expectedSql = $@"SELECT 't'.'Id', 't'.'Description'
                                FROM 'TodoItems' AS 't'
                                WHERE 't'.'Id' = {todoItem.Id}".Normalize();

            // act
            var query = _dbContext
                .TodoItems
                .Where(t=>t.Id == todoItem.Id)
                .Select(fields);

            var resultSql = query.ToSql().Normalize();
            var result = await query.FirstAsync();

            // assert
            Assert.Equal(0, result.Ordinal);
            Assert.Equal(todoItem.Description, result.Description);
            Assert.Equal(expectedSql, resultSql);
        }

        [Fact]
        public async Task Fields_Query_Selects_Sparse_Field_Sets()
        {
            // arrange
            var todoItem = new TodoItem {
                Description = "description",
                Ordinal = 1
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();
            
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?fields[todo-items]=description";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // assert
            Assert.Equal(todoItem.StringId, deserializeBody.Data.Id);
            Assert.Equal(1, deserializeBody.Data.Attributes.Count);
            Assert.Equal(todoItem.Description, deserializeBody.Data.Attributes["description"]);
        }
    }
}
