using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Models;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCoreExample.Data;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Serialization;
using System.Linq;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class AttributeFilterTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private Faker<TodoItem> _todoItemFaker;
        
        public AttributeFilterTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _todoItemFaker = new Faker<TodoItem>()
                .RuleFor(t => t.Description, f => f.Lorem.Sentence())
                .RuleFor(t => t.Ordinal, f => f.Random.Number());
        }

        [Fact]
        public async Task Can_Filter_On_Guid_Properties()
        {
            // arrange
            var context = _fixture.GetService<AppDbContext>();
            var todoItem = _todoItemFaker.Generate();
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
            
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?filter[guid-property]={todoItem.GuidProperty}";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture
                .GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(body);

            var todoItemResponse = deserializedBody.Single();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Id, todoItemResponse.Id);
            Assert.Equal(todoItem.GuidProperty, todoItemResponse.GuidProperty);
        }
    }
}
