using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Spec
{
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
        
        [Fact]
        public async Task Patching_Resource_With_Mismatching_Resource_Type_Returns_Conflict()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    type = "people",
                    id = 1
                }
            });

            // Act
            var (body, _) = await Patch("/api/v1/todoItems/1", content);

            // Assert
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Conflict, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Resource type mismatch between request body and endpoint URL.", errorDocument.Errors[0].Title);
            Assert.Equal("Expected resource of type 'todoItems' in PATCH request body at endpoint '/api/v1/todoItems/1', instead of 'people'.", errorDocument.Errors[0].Detail);
        }

        [Fact]
        public async Task Patching_Through_Relationship_Link_With_Mismatching_Resource_Type_Returns_Conflict()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    type = "todoItems",
                    id = 1
                }
            });

            // Act
            var (body, _) = await Patch("/api/v1/todoItems/1/relationships/owner", content);

            // Assert
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Conflict, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Resource type mismatch between request body and endpoint URL.", errorDocument.Errors[0].Title);
            Assert.Equal("Expected resource of type 'people' in PATCH request body at endpoint '/api/v1/todoItems/1/relationships/owner', instead of 'todoItems'.", errorDocument.Errors[0].Detail);
        }
        
        [Fact]
        public async Task Patching_Through_Relationship_Link_With_Multiple_Resources_Types_Returns_Conflict()
        {
            // Arrange
            string content = JsonConvert.SerializeObject(new
            {
                data = new[]
                {
                    new { type = "todoItems", id = 1 },
                    new { type = "articles", id = 2 },
                }
            });

            // Act
            var (body, _) = await Patch("/api/v1/todoItems/1/relationships/childrenTodos", content);

            // Assert
            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.Conflict, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Resource type mismatch between request body and endpoint URL.", errorDocument.Errors[0].Title);
            Assert.Equal("Expected resource of type 'todoItems' in PATCH request body at endpoint '/api/v1/todoItems/1/relationships/childrenTodos', instead of 'articles'.", errorDocument.Errors[0].Detail);
        }
    }
}
