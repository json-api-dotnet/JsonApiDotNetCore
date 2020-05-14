using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    [Collection("WebHostCollection")]
    public sealed class ActionResultTests
    {
        private readonly TestFixture<TestStartup> _fixture;

        public ActionResultTests(TestFixture<TestStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ActionResult_With_Error_Object_Is_Converted_To_Error_Collection()
        {
            // Arrange
            var route = "/abstract";
            var request = new HttpRequestMessage(HttpMethod.Post, route);
            var content = new
            {
                data = new
                {
                    type = "todoItems",
                    id = 1,
                    attributes = new Dictionary<string, object>
                    {
                        {"ordinal", 1}
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("NotFound ActionResult with explicit error object.", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Empty_ActionResult_Is_Converted_To_Error_Collection()
        {
            // Arrange
            var route = "/abstract/123";
            var request = new HttpRequestMessage(HttpMethod.Delete, route);

            // Act
            var response = await _fixture.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.NotFound, errorDocument.Errors[0].StatusCode);
            Assert.Equal("NotFound", errorDocument.Errors[0].Title);
            Assert.Null(errorDocument.Errors[0].Detail);
        }
    }
}
