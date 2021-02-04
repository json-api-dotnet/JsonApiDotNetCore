using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    public sealed class ActionResultTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<ActionResultDbContext>, ActionResultDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<ActionResultDbContext>, ActionResultDbContext> _testContext;

        public ActionResultTests(ExampleIntegrationTestContext<TestableStartup<ActionResultDbContext>, ActionResultDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_resource_by_ID()
        {
            // Arrange
            var toothbrush = new Toothbrush();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Toothbrushes.Add(toothbrush);
                await dbContext.SaveChangesAsync();
            });

            var route = "/toothbrushes/" + toothbrush.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(toothbrush.StringId);
        }

        [Fact]
        public async Task Converts_empty_ActionResult_to_error_collection()
        {
            // Arrange
            var route = "/toothbrushes/11111111";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("NotFound");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Converts_ActionResult_with_error_object_to_error_collection()
        {
            // Arrange
            var route = "/toothbrushes/22222222";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("No toothbrush with that ID exists.");
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_convert_ActionResult_with_string_parameter_to_error_collection()
        {
            // Arrange
            var route = "/toothbrushes/33333333";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            responseDocument.Errors[0].Title.Should().Be("An unhandled error occurred while processing this request.");
            responseDocument.Errors[0].Detail.Should().Be("Data being returned must be errors or resources.");
        }

        [Fact]
        public async Task Converts_ObjectResult_with_error_object_to_error_collection()
        {
            // Arrange
            var route = "/toothbrushes/44444444";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadGateway);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadGateway);
            responseDocument.Errors[0].Title.Should().BeNull();
            responseDocument.Errors[0].Detail.Should().BeNull();
        }

        [Fact]
        public async Task Converts_ObjectResult_with_error_objects_to_error_collection()
        {
            // Arrange
            var route = "/toothbrushes/55555555";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(3);

            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            responseDocument.Errors[0].Title.Should().BeNull();
            responseDocument.Errors[0].Detail.Should().BeNull();

            responseDocument.Errors[1].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            responseDocument.Errors[1].Title.Should().BeNull();
            responseDocument.Errors[1].Detail.Should().BeNull();

            responseDocument.Errors[2].StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
            responseDocument.Errors[2].Title.Should().Be("This is not a very great request.");
            responseDocument.Errors[2].Detail.Should().BeNull();
        }
    }
}
