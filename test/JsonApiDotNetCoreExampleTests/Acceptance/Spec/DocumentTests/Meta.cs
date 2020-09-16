using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Definitions;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public sealed class Meta
    {
        private readonly TestFixture<TestStartup> _fixture;
        private readonly AppDbContext _context;
        public Meta(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Total_Resource_Count_Included()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            _context.TodoItems.Add(new TodoItem());
            await _context.SaveChangesAsync();
            var expectedCount = 1;
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(documents.Meta);
            Assert.Equal(expectedCount, (long)documents.Meta["totalResources"]);
        }

        [Fact]
        public async Task Total_Resource_Count_Included_When_None()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(documents.Meta);
            Assert.Equal(0, (long)documents.Meta["totalResources"]);
        }

        [Fact]
        public async Task Total_Resource_Count_Not_Included_In_POST_Response()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var httpMethod = new HttpMethod("POST");
            var route = "/api/v1/todoItems";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    attributes = new
                    {
                        description = "New Description"
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(documents.Meta == null || !documents.Meta.ContainsKey("totalResources"));
        }

        [Fact]
        public async Task Total_Resource_Count_Not_Included_In_PATCH_Response()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            TodoItem todoItem = new TodoItem();
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var httpMethod = new HttpMethod("PATCH");
            var route = $"/api/v1/todoItems/{todoItem.Id}";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = todoItem.Id,
                    attributes = new
                    {
                        description = "New Description"
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(documents.Meta == null || !documents.Meta.ContainsKey("totalResources"));
        }

        [Fact]
        public async Task ResourceThatImplements_IHasMeta_Contains_MetaData()
        {
            // Arrange
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var expectedMeta = _fixture.GetService<IResourceDefinition<Person>>().GetMeta();

            // Act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(documents.Meta);
            Assert.NotNull(expectedMeta);
            Assert.NotEmpty(expectedMeta);

            foreach (var hash in expectedMeta)
            {
                if (hash.Value is IList listValue)
                {
                    for (var i = 0; i < listValue.Count; i++)
                        Assert.Equal(listValue[i].ToString(), ((IList)documents.Meta[hash.Key])[i].ToString());
                }
                else
                {
                    Assert.Equal(hash.Value, documents.Meta[hash.Key]);
                }
            }
        }
    }
}
