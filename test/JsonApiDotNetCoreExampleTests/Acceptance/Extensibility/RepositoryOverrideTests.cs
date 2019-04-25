using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Services;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public class RepositoryOverrideTests
    {
        private TestFixture<TestStartup> _fixture;

        public RepositoryOverrideTests(TestFixture<TestStartup> fixture)
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

        [Fact]
        public async Task Sparse_Fields_Works_With_Get_Override()
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
            var todoItem = new TodoItem();
            todoItem.Owner = person;
            context.TodoItems.Add(todoItem);
            context.SaveChanges();

            var authService = (IAuthorizationService)server.Host.Services.GetService(typeof(IAuthorizationService));
            authService.CurrentUserId = person.Id;

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items/{todoItem.Id}?fields[todo-items]=description";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _fixture.GetService<IJsonApiDeSerializer>().Deserialize<TodoItem>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoItem.Description, deserializedBody.Description);

        }
    }
}
