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
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class Relationships
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;
        public Relationships(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_ManyToOne_Relationships()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var data = documents.Data[0];
            var expectedOwnerSelfLink = $"http://localhost/api/v1/todo-items/{data.Id}/relationships/owner";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/todo-items/{data.Id}/owner";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["owner"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["owner"].Links.Related);
        }

        [Fact]
        public async Task Correct_RelationshipObjects_For_OneToMany_Relationships()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            var data = documents.Data[0];
            var expectedOwnerSelfLink = $"http://localhost/api/v1/people/{data.Id}/relationships/todo-items";
            var expectedOwnerRelatedLink = $"http://localhost/api/v1/people/{data.Id}/todo-items";

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedOwnerSelfLink, data.Relationships["todo-items"].Links.Self);
            Assert.Equal(expectedOwnerRelatedLink, data.Relationships["todo-items"].Links.Related);
        }
    }
}
