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
using System.Linq;
using JsonApiDotNetCoreExampleTests.Startups;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec.DocumentTests
{
    [Collection("WebHostCollection")]
    public class Meta
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        private AppDbContext _context;
        public Meta(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
            _context = fixture.GetService<AppDbContext>();
        }

        [Fact]
        public async Task Total_Record_Count_Included()
        {
            // arrange
            var expectedCount = _context.TodoItems.Count();
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(documents.Meta);
            Assert.Equal((long)expectedCount, (long)documents.Meta["total-records"]);
        }
    }
}
