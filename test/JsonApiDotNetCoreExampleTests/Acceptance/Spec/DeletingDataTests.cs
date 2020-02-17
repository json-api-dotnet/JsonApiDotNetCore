using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class DeletingDataTests
    {
        private AppDbContext _context;

        public DeletingDataTests(TestFixture<Startup> fixture)
        {
            _context = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Respond_404_If_EntityDoesNotExist()
        {
            // Arrange
            var lastTodo = _context.TodoItems.AsEnumerable().LastOrDefault();
            var lastTodoId = lastTodo?.Id ?? 0;

            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var httpMethod = new HttpMethod("DELETE");
            var route = $"/api/v1/todoItems/{lastTodoId + 100}";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
