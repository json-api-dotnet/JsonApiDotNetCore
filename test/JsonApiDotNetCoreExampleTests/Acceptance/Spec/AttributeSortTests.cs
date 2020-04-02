using JsonApiDotNetCoreExample;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class AttributeSortTests
    {
        private readonly TestFixture<Startup> _fixture;

        public AttributeSortTests(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Sort_If_Explicitly_Forbidden()
        {
            // Arrange
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/todoItems?include=owner&sort=achievedDate";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].Status);
            Assert.Equal("Sorting on the requested attribute is not allowed.", errorDocument.Errors[0].Title);
            Assert.Equal("Sorting on attribute 'achievedDate' is not allowed.", errorDocument.Errors[0].Detail);
            Assert.Equal("sort", errorDocument.Errors[0].Source.Parameter);
        }
    }
}
