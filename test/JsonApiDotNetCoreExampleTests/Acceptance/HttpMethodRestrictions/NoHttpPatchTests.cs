using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public class NoHttpPatchTests
    {
        [Fact]
        public async Task Allows_GET_Requests()
        {
            // Arrange
            const string route = "nohttppatch";
            const string method = "GET";

            // Act
            var statusCode = await MakeRequestAsync(route, method);

            // Assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        [Fact]
        public async Task Allows_POST_Requests()
        {
            // Arrange
            const string route = "nohttppatch";
            const string method = "POST";

            // Act
            var statusCode = await MakeRequestAsync(route, method);

            // Assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        [Fact]
        public async Task Rejects_PATCH_Requests()
        {
            // Arrange
            const string route = "nohttppatch";
            const string method = "PATCH";

            // Act
            var statusCode = await MakeRequestAsync(route, method);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
        }

        [Fact]
        public async Task Allows_DELETE_Requests()
        {
            // Arrange
            const string route = "nohttppatch";
            const string method = "DELETE";

            // Act
            var statusCode = await MakeRequestAsync(route, method);

            // Assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        private async Task<HttpStatusCode> MakeRequestAsync(string route, string method)
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var httpMethod = new HttpMethod(method);
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);
            var response = await client.SendAsync(request);
            return response.StatusCode;
        }
    }
}
