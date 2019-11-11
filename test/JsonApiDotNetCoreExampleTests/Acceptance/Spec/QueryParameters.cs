using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class QueryParameters
    {
        private TestFixture<Startup> _fixture;
        public QueryParameters(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Server_Returns_400_ForUnknownQueryParam()
        {
            // Arrange
            const string queryKey = "unknownKey";
            const string queryValue = "value";
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todoItems?{queryKey}={queryValue}";
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var errorCollection = JsonConvert.DeserializeObject<ErrorCollection>(body);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(errorCollection.Errors);
            Assert.Equal($"[{queryKey}, {queryValue}] is not a valid query.", errorCollection.Errors[0].Title);
        }
    }
}
