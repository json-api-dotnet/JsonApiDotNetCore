using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Meta
    {
        private TestFixture<Startup> _fixture;
        private AppDbContext _context;
        public Meta(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Total_Record_Count_Included()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.TodoItems.Add(new TodoItem());
            _context.SaveChanges();
            var expectedCount = 1;
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

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
            Assert.Equal(expectedCount, (long)documents.Meta["total-records"]);
        }

        [Fact]
        public async Task Total_Record_Count_Included_When_None()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.SaveChanges();
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

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
            Assert.Equal(0, (long)documents.Meta["total-records"]);
        }

        [Fact]
        public async Task Total_Record_Count_Not_Included_In_POST_Response()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.SaveChanges();
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

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
                        description = "New Description",
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.False(documents.Meta.ContainsKey("total-records"));
        }

        [Fact]
        public async Task Total_Record_Count_Not_Included_In_PATCH_Response()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);
            TodoItem todoItem = new TodoItem();
            _context.TodoItems.Add(todoItem);
            _context.SaveChanges();
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

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
                        description = "New Description",
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            // Act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var documents = JsonConvert.DeserializeObject<Document>(responseBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(documents.Meta.ContainsKey("total-records"));
        }

        [Fact]
        public async Task EntityThatImplements_IHasMeta_Contains_MetaData()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var expectedMeta = (_fixture.GetService<ResourceDefinition<Person>>() as IHasMeta).GetMeta();

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
