using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    public class ThrowingResourceTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        public ThrowingResourceTests(StandardApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetThrowingResource_Fails()
        {
            // Arrange
            var throwingResource = new ThrowingResource();
            _dbContext.Add(throwingResource);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"/api/v1/throwingResources/{throwingResource.Id}");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.InternalServerError, response);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);

            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.InternalServerError, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Failed to serialize response body.", errorDocument.Errors[0].Title);
            Assert.Equal("The value for the 'FailsOnSerialize' property is currently unavailable.", errorDocument.Errors[0].Detail);

            var stackTraceLines =
                ((JArray) errorDocument.Errors[0].Meta.Data["stackTrace"]).Select(token => token.Value<string>());
            
            Assert.Contains(stackTraceLines, line => line.Contains(
                "System.InvalidOperationException: The value for the 'FailsOnSerialize' property is currently unavailable."));
        }
    }
}
