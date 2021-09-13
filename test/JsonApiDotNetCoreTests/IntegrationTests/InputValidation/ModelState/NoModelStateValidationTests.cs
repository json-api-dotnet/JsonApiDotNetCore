using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState
{
    public sealed class NoModelStateValidationTests : IClassFixture<IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;

        public NoModelStateValidationTests(IntegrationTestContext<TestableStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<SystemDirectoriesController>();
            testContext.UseController<SystemFilesController>();
        }

        [Fact]
        public async Task Can_create_resource_with_invalid_attribute_value()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    attributes = new
                    {
                        name = "!@#$%^&*().-",
                        isCaseSensitive = false
                    }
                }
            };

            const string route = "/systemDirectories";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes["name"].Should().Be("!@#$%^&*().-");
        }

        [Fact]
        public async Task Can_update_resource_with_invalid_attribute_value()
        {
            // Arrange
            var directory = new SystemDirectory
            {
                Name = "Projects",
                IsCaseSensitive = false
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Directories.Add(directory);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "systemDirectories",
                    id = directory.StringId,
                    attributes = new
                    {
                        name = "!@#$%^&*().-"
                    }
                }
            };

            string route = $"/systemDirectories/{directory.StringId}";

            // Act
            (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();
        }
    }
}
