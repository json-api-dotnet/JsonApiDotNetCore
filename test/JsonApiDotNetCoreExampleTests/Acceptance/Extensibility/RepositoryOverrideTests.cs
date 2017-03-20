using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExampleTests.Startups;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Services;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using DotNetCoreDocs;
using JsonApiDotNetCoreExample;
using DotNetCoreDocs.Writers;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public class RepositoryOverrideTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;

        public RepositoryOverrideTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Total_Record_Count_Included()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<AuthorizedStartup>();
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var context = (AppDbContext)server.Host.Services.GetService(typeof(AppDbContext));
            var jsonApiContext = (IJsonApiContext)server.Host.Services.GetService(typeof(IJsonApiContext));

            var person = new Person();
            context.People.Add(person);
            var ownedTodoItem = new TodoItem();
            var unOwnedTodoItem = new TodoItem();
            ownedTodoItem.Owner = person;
            context.TodoItems.Add(ownedTodoItem);
            context.TodoItems.Add(unOwnedTodoItem);
            context.SaveChanges(); 

            var authService =  (IAuthorizationService)server.Host.Services.GetService(typeof(IAuthorizationService));        
            authService.CurrentUserId = person.Id;

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner";
            
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().DeserializeList<TodoItem>(responseBody);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            foreach(var item in deserializedBody)
                Assert.Equal(person.Id, item.OwnerId);
        }
    }
}
