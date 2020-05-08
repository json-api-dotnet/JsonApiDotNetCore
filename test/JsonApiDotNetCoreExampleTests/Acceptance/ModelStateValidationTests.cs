using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Acceptance.Spec;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public sealed class ModelStateValidationTests : FunctionalTestCollection<StandardApplicationFactory>
    {
        public ModelStateValidationTests(StandardApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task When_posting_tag_with_invalid_name_it_must_fail()
        {
            // Arrange
            var tag = new Tag(_dbContext)
            {
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(tag);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tags")
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = true;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("The field Name must match the regular expression '^\\W$'.", errorDocument.Errors[0].Detail);
            Assert.Equal("/data/attributes/name", errorDocument.Errors[0].Source.Pointer);
        }

        [Fact]
        public async Task When_posting_tag_with_invalid_name_without_model_state_validation_it_must_succeed()
        {
            // Arrange
            var tag = new Tag(_dbContext)
            {
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(tag);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tags")
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = false;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task When_patching_tag_with_invalid_name_it_must_fail()
        {
            // Arrange
            var existingTag = new Tag(_dbContext)
            {
                Name = "Technology"
            };

            var context = _factory.GetService<AppDbContext>();
            context.Tags.Add(existingTag);
            context.SaveChanges();

            var updatedTag = new Tag(_dbContext)
            {
                Id = existingTag.Id,
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(updatedTag);

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/tags/" + existingTag.StringId)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = true;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var errorDocument = JsonConvert.DeserializeObject<ErrorDocument>(body);
            Assert.Single(errorDocument.Errors);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, errorDocument.Errors[0].StatusCode);
            Assert.Equal("Input validation failed.", errorDocument.Errors[0].Title);
            Assert.Equal("The field Name must match the regular expression '^\\W$'.", errorDocument.Errors[0].Detail);
            Assert.Equal("/data/attributes/name", errorDocument.Errors[0].Source.Pointer);
        }

        [Fact]
        public async Task When_patching_tag_with_invalid_name_without_model_state_validation_it_must_succeed()
        {
            // Arrange
            var existingTag = new Tag(_dbContext)
            {
                Name = "Technology"
            };

            var context = _factory.GetService<AppDbContext>();
            context.Tags.Add(existingTag);
            context.SaveChanges();

            var updatedTag = new Tag(_dbContext)
            {
                Id = existingTag.Id,
                Name = "!@#$%^&*().-"
            };

            var serializer = GetSerializer<Tag>();
            var content = serializer.Serialize(updatedTag);

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/tags/" + existingTag.StringId)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);

            var options = (JsonApiOptions)_factory.GetService<IJsonApiOptions>();
            options.ValidateModelState = false;

            // Act
            var response = await _factory.Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
