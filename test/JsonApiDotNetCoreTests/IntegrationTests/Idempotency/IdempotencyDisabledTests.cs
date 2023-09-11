using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

public sealed class IdempotencyDisabledTests : IClassFixture<IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> _testContext;
    private readonly IdempotencyFakers _fakers = new();

    public IdempotencyDisabledTests(IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TreesController>();
    }

    [Fact]
    public async Task Cannot_create_resource_with_idempotency_key_when_disabled()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route = "/trees";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().Be("Idempotency is currently disabled.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }
}
