using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

public sealed class ApiControllerAttributeTests : IClassFixture<IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> _testContext;

    public ApiControllerAttributeTests(IntegrationTestContext<TestableStartup<CustomRouteDbContext>, CustomRouteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CiviliansController>();
    }

    [Fact]
    public async Task ApiController_attribute_transforms_NotFound_action_result_without_arguments_into_ProblemDetails()
    {
        // Arrange
        const string route = "/world-civilians/missing";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.Links.ShouldNotBeNull();
        error.Links.About.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
    }

    [Fact]
    public async Task ProblemDetails_from_invalid_ModelState_is_translated_into_error_response()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "civilians",
                attributes = new
                {
                    name = (string?)null,
                    yearOfBirth = 1850
                }
            }
        };

        const string route = "/world-civilians";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(2);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error1.Links.ShouldNotBeNull();
        error1.Links.About.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        error1.Title.Should().Be("One or more validation errors occurred.");
        error1.Detail.Should().Be("The Name field is required.");
        error1.Source.Should().BeNull();

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error2.Links.ShouldNotBeNull();
        error2.Links.About.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        error2.Title.Should().Be("One or more validation errors occurred.");
        error2.Detail.Should().Be("The field YearOfBirth must be between 1900 and 2050.");
        error2.Source.Should().BeNull();
    }
}
