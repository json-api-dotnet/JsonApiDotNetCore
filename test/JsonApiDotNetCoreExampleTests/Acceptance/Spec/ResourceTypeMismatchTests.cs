using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
    // TODO: Move left-over tests in this file.

    public sealed class ResourceTypeMismatchTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        public ResourceTypeMismatchTests(StandardApplicationFactory factory) : base(factory) { }
        
        [Fact]
        public async Task Posting_Resource_With_Mismatching_Resource_Type_Returns_Conflict()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    type = "people"
                }
            });

            // Act
            var (body, _) = await Post("/api/v1/todoItems", content);

            // Assert
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Conflict, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Resource type mismatch between request body and endpoint URL.", errorDocument.Errors[0].Title);
            Assert.Equal("Expected resource of type 'todoItems' in POST request body at endpoint '/api/v1/todoItems', instead of 'people'.", errorDocument.Errors[0].Detail);
        }
    }
}
