using JsonApiDotNetCoreExample;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public class AttributeSortTests
    {
        private TestFixture<Startup> _fixture;

        public AttributeSortTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Sort_If_Explicitly_Forbidden()
        {
            // Arrange
            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/todo-items?include=owner&sort=achieved-date";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
