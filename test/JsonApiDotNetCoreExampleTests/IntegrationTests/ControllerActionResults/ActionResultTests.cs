using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    public sealed class ActionResultTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<ActionResultDbContext>, ActionResultDbContext>>
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

            string route = "/toothbrushes/" + toothbrush.StringId;

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(toothbrush.StringId);
        }

        [Fact]
        public async Task Converts_empty_ActionResult_to_error_collection()
        {
            // Arrange
            string route = "/toothbrushes/" + BaseToothbrushesController.EmptyActionResultId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("NotFound");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Converts_ActionResult_with_error_object_to_error_collection()
        {
            // Arrange
            string route = "/toothbrushes/" + BaseToothbrushesController.ActionResultWithErrorObjectId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.NotFound);
            error.Title.Should().Be("No toothbrush with that ID exists.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Cannot_convert_ActionResult_with_string_parameter_to_error_collection()
        {
            // Arrange
            string route = "/toothbrushes/" + BaseToothbrushesController.ActionResultWithStringParameter;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing this request.");
            error.Detail.Should().Be("Data being returned must be errors or resources.");
        }

        [Fact]
        public async Task Converts_ObjectResult_with_error_object_to_error_collection()
        {
            // Arrange
            string route = "/toothbrushes/" + BaseToothbrushesController.ObjectResultWithErrorObjectId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadGateway);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadGateway);
            error.Title.Should().BeNull();
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Converts_ObjectResult_with_error_objects_to_error_collection()
        {
            // Arrange
            string route = "/toothbrushes/" + BaseToothbrushesController.ObjectResultWithErrorCollectionId;

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(3);

            Error error1 = responseDocument.Errors[0];
            error1.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            error1.Title.Should().BeNull();
            error1.Detail.Should().BeNull();

            Error error2 = responseDocument.Errors[1];
            error2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            error2.Title.Should().BeNull();
            error2.Detail.Should().BeNull();

            Error error3 = responseDocument.Errors[2];
            error3.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
            error3.Title.Should().Be("This is not a very great request.");
            error3.Detail.Should().BeNull();
        }
    }
}
