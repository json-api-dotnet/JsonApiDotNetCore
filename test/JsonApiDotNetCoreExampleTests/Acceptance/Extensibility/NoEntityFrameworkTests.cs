using JsonApiDotNetCore.Serialization;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample;
using System.Net.Http;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using System.Threading.Tasks;
using System.Net;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    public class NoEntityFrameworkTests
    {
        [Fact]
        public async Task Can_Implement_Custom_IResourceService_Without_EFAsync()
        {
            // arrange
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }
    }
}
