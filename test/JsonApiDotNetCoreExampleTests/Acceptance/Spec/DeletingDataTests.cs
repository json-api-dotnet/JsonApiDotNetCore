using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
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

        public DeletingDataTests(TestFixture<Startup> fixture)
        {
            _context = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Respond_404_If_EntityDoesNotExist()
        {
            // Arrange
            _context.TodoItems.RemoveRange(_context.TodoItems);
            await _context.SaveChangesAsync();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

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
            Assert.Equal("Resource of type 'todoItems' with id '123' does not exist.",errorDocument.Errors[0].Detail);
        }
    }
}
