using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

public sealed class IdempotencyOperationTests : IClassFixture<IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> _testContext;
    private readonly IdempotencyFakers _fakers = new();

    public IdempotencyOperationTests(IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddScoped<IIdempotencyProvider, IdempotencyProvider>();
            services.AddScoped<ISystemClock, FrozenSystemClock>();
        });
    }

    [Fact]
    public async Task Returns_cached_response_for_operations_request()
    {
        // Arrange
        Branch existingBranch = _fakers.Branch.Generate();
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Branches.Add(existingBranch);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "branches",
                        id = existingBranch.StringId
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "trees",
                        attributes = new
                        {
                            heightInMeters = newHeightInMeters
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePostAtomicAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePostAtomicAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.OK);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().Be(idempotencyKey.DoubleQuote());

        httpResponse2.Content.Headers.ContentType.Should().Be(httpResponse1.Content.Headers.ContentType);
        httpResponse2.Content.Headers.ContentLength.Should().Be(httpResponse1.Content.Headers.ContentLength);

        responseDocument2.Should().Be(responseDocument1);
    }

    [Fact]
    public async Task Returns_cached_response_for_failed_operations_request()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "branches",
                        id = Unknown.StringId.For<Branch, long>()
                    }
                }
            }
        };

        const string route = "/operations";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePostAtomicAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePostAtomicAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().Be(idempotencyKey.DoubleQuote());

        httpResponse2.Content.Headers.ContentType.Should().Be(httpResponse1.Content.Headers.ContentType);
        httpResponse2.Content.Headers.ContentLength.Should().Be(httpResponse1.Content.Headers.ContentLength);

        responseDocument2.Should().Be(responseDocument1);
    }
}
