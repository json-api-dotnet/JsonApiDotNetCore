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
    public class HttpReadOnlyTests
    {
        [Fact]
        public async Task Allows_GET_Requests()
        {
            // arrange
            const string route = "readonly";
            const string method = "GET";

            // act
            var statusCode = await MakeRequestAsync(route, method);

            // assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        [Fact]
        public async Task Rejects_POST_Requests()
        {
            // arrange
            const string route = "readonly";
            const string method = "POST";

            // act
            var statusCode = await MakeRequestAsync(route, method);

            // assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
        }

        [Fact]
        public async Task Rejects_PATCH_Requests()
        {
            // arrange
            const string route = "readonly";
            const string method = "PATCH";

            // act
            var statusCode = await MakeRequestAsync(route, method);

            // assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
        }

        [Fact]
        public async Task Rejects_DELETE_Requests()
        {
            // arrange
            const string route = "readonly";
            const string method = "DELETE";

            // act
            var statusCode = await MakeRequestAsync(route, method);

            // assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, statusCode);
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