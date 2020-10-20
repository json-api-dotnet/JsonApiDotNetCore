using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class DeletingDataTests
    {
        private readonly AppDbContext _context;

        public DeletingDataTests(TestFixture<TestStartup> fixture)
        {
            _context = fixture.GetRequiredService<AppDbContext>();
        }

        [Fact]
        public async Task Respond_404_If_ResourceDoesNotExist()
        {
            // Arrange
            await _context.ClearTableAsync<TodoItem>();
            await _context.SaveChangesAsync();

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var httpMethod = new HttpMethod("DELETE");
            var route = "/api/v1/todoItems/123";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("The requested resource does not exist.", errorDocument.Errors[0].Title);
            Assert.Equal("Resource of type 'todoItems' with ID '123' does not exist.",errorDocument.Errors[0].Detail);
        }

        // TODO: Add test for DeleteRelationshipAsync that only deletes non-existing from the right resources in to-many relationship.
    }
}
