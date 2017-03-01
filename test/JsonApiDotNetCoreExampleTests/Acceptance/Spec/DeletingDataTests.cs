using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bogus;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class DeletingDataTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        private Faker<TodoItem> _todoItemFaker;

        public DeletingDataTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
        }

        [Fact]
        public async Task Respond_404_If_EntityDoesNotExist()
        {
            // arrange
            var maxPersonId = _context.TodoItems.LastOrDefault()?.Id ?? 0;
            var todoItem = _todoItemFaker.Generate();
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();
    
            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/todo-items/{maxPersonId + 100}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
