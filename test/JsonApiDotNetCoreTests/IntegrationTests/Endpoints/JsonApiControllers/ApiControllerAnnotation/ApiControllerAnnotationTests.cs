using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

public sealed class ApiControllerAnnotationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ApiControllerAnnotationDbContext>, ApiControllerAnnotationDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ApiControllerAnnotationDbContext>, ApiControllerAnnotationDbContext> _testContext;
    private readonly ApiControllerAnnotationFakers _fakers = new();

    public ApiControllerAnnotationTests(IntegrationTestContext<TestableStartup<ApiControllerAnnotationDbContext>, ApiControllerAnnotationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<LoginTokensController>();
    }

    [Fact]
    public async Task Can_get_primary_resource()
    {
        // Arrange
        LoginToken loginToken = _fakers.LoginToken.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.LoginTokens.Add(loginToken);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/loginTokens/{loginToken.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("loginTokens");
        responseDocument.Data.SingleValue.Id.Should().Be(loginToken.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("value").WhoseValue.Should().Be(loginToken.Value);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("createdAt").WhoseValue.Should().Be(loginToken.CreatedAt);
    }

    [Fact]
    public async Task ApiController_attribute_transforms_NotFound_action_result_without_arguments_into_ProblemDetails()
    {
        // Arrange
        const string route = "/loginTokens/missing";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Links.Should().NotBeNull();
        error.Links.About.Should().StartWith("https://tools.ietf.org/html/rfc");
    }

    [Fact]
    public async Task ProblemDetails_from_invalid_ModelState_is_translated_into_error_response()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "loginTokens",
                attributes = new
                {
                    value = (string?)null,
                    createdAt = (DateTimeOffset?)null
                }
            }
        };

        const string route = "/loginTokens";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error1.Links.Should().NotBeNull();
        error1.Links.About.Should().StartWith("https://tools.ietf.org/html/rfc");
        error1.Title.Should().Be("One or more validation errors occurred.");
        error1.Detail.Should().Be("The Value field is required.");
        error1.Source.Should().BeNull();

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error2.Links.Should().NotBeNull();
        error2.Links.About.Should().StartWith("https://tools.ietf.org/html/rfc");
        error2.Title.Should().Be("One or more validation errors occurred.");
        error2.Detail.Should().Be("The CreatedAt field is required.");
        error2.Source.Should().BeNull();
    }
}
