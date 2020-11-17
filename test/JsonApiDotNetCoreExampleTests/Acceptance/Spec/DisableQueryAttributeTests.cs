using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    [Collection("WebHostCollection")]
    public sealed class DisableQueryAttributeTests
    {
        private readonly TestFixture<TestStartup> _fixture;

        public DisableQueryAttributeTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Cannot_Sort_If_Query_String_Parameter_Is_Blocked_By_Controller()
        {
            // Arrange
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/countries?sort=name";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Usage of one or more query string parameters is not allowed at the requested endpoint.", errorDocument.Errors[0].Title);
            Assert.Equal("The parameter 'sort' cannot be used at this endpoint.", errorDocument.Errors[0].Detail);
            Assert.Equal("sort", errorDocument.Errors[0].Source.Parameter);
        }

        [Fact]
        public async Task Cannot_Use_Custom_Query_String_Parameter_If_Blocked_By_Controller()
        {
            // Arrange
            var httpMethod = new HttpMethod("GET");
            var route = "/api/v1/tags?skipCache=true";
            var request = new HttpRequestMessage(httpMethod, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Usage of one or more query string parameters is not allowed at the requested endpoint.", errorDocument.Errors[0].Title);
            Assert.Equal("The parameter 'skipCache' cannot be used at this endpoint.", errorDocument.Errors[0].Detail);
            Assert.Equal("skipCache", errorDocument.Errors[0].Source.Parameter);
        }
    }
}
