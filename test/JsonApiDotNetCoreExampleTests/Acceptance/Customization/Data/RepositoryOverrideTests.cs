using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetCoreDocs;
using DotNetCoreDocs.Writers;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExampleTests.Startups;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Services;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class RepositoryOverrideTests
    {
        public RepositoryOverrideTests()
        { }

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
            var deserializedBody = JsonApiDeSerializer.DeserializeList<TodoItem>(responseBody, jsonApiContext);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            foreach(var item in deserializedBody)
                Assert.Equal(person.Id, item.OwnerId);
        }
    }
}
