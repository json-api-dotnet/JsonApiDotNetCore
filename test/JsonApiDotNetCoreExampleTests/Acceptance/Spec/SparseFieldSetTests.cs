using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using StringExtensions = JsonApiDotNetCoreExampleTests.Helpers.Extensions.StringExtensions;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class SparseFieldSetTests
    {
        private TestFixture<Startup> _fixture;
        private readonly AppDbContext _dbContext;

        public SparseFieldSetTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _dbContext = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Can_Select_Sparse_Fieldsets()
        {
            // arrange
            var fields = new List<string> { "Id", "Description", "CreatedDate", "AchievedDate" };
            var todoItem = new TodoItem {
                Description = "description",
                Ordinal = 1,
                CreatedDate = DateTime.Now,
                AchievedDate = DateTime.Now.AddDays(2)
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();
            var expectedSql = StringExtensions.Normalize($@"SELECT 't'.'Id', 't'.'Description', 't'.'CreatedDate', 't'.'AchievedDate'
                                FROM 'TodoItems' AS 't'
                                WHERE 't'.'Id' = {todoItem.Id}");

            // act
            var query = _dbContext
                .TodoItems
                .Where(t=>t.Id == todoItem.Id)
                .Select(fields);

            var resultSql = StringExtensions.Normalize(query.ToSql());
            var result = await query.FirstAsync();

            // assert
            Assert.Equal(0, result.Ordinal);
            Assert.Equal(todoItem.Description, result.Description);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), result.CreatedDate.ToString("G"));
            Assert.Equal(todoItem.AchievedDate.GetValueOrDefault().ToString("G"), result.AchievedDate.GetValueOrDefault().ToString("G"));
            Assert.Equal(expectedSql, resultSql);
        }

        [Fact]
        public async Task Fields_Query_Selects_Sparse_Field_Sets()
        {
            // arrange
            var todoItem = new TodoItem {
                Description = "description",
                Ordinal = 1, 
                CreatedDate = DateTime.Now
            };
            _dbContext.TodoItems.Add(todoItem);
            await _dbContext.SaveChangesAsync();
            
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var route = $"/api/v1/todo-items/{todoItem.Id}?fields[todo-items]=description,created-date";
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializeBody = JsonConvert.DeserializeObject<Document>(body);

            // assert
            Assert.Equal(todoItem.StringId, deserializeBody.Data.Id);
            Assert.Equal(2, deserializeBody.Data.Attributes.Count);
            Assert.Equal(todoItem.Description, deserializeBody.Data.Attributes["description"]);
            Assert.Equal(todoItem.CreatedDate.ToString("G"), ((DateTime)deserializeBody.Data.Attributes["created-date"]).ToString("G"));
        }
    }
}
