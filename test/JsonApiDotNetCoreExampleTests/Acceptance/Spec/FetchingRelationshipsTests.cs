using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class FetchingRelationshipsTests
    {
        private TestFixture<Startup> _fixture;
        private IJsonApiContext _jsonApiContext;
        private Faker<TodoItem> _todoItemFaker;

        public FetchingRelationshipsTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _jsonApiContext = fixture.GetService<IJsonApiContext>();
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number())
                .RuleFor(t => t.CreatedDate, f => f.Date.Past());
        }

        [Fact]
        public async Task Request_UnsetRelationship_Returns_Null_DataObject()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}/owner";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var expectedBody = "{\"data\":null}";

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.api+json", response.Content.Headers.ContentType.ToString());
            Assert.Equal(expectedBody, body);

            context.Dispose();
        }

        [Fact]
        public async Task Request_ForRelationshipLink_ThatDoesNotExist_Returns_404()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();

            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var todoItemId = todoItem.Id;
            context.TodoItems.Remove(todoItem);
            await context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItemId}/owner";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            context.Dispose();
        }
    }
}
