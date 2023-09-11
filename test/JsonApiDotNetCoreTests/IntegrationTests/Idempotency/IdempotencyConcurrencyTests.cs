using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

public sealed class IdempotencyConcurrencyTests : IClassFixture<IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> _testContext;
    private readonly IdempotencyFakers _fakers = new();

    public IdempotencyConcurrencyTests(IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<LeavesController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddScoped<IIdempotencyProvider, IdempotencyProvider>();
            services.AddScoped<ISystemClock, FrozenSystemClock>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<TestExecutionMediator>();
            services.AddResourceDefinition<LeafSignalingDefinition>();
        });
    }

    [Fact]
    public async Task Cannot_create_resource_concurrently_with_same_idempotency_key()
    {
        // Arrange
        Branch existingBranch = _fakers.Branch.Generate();
        string newColor = _fakers.Leaf.Generate().Color;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Branches.Add(existingBranch);
            await dbContext.SaveChangesAsync();
        });

        var mediator = _testContext.Factory.Services.GetRequiredService<TestExecutionMediator>();

        var requestBody = new
        {
            data = new
            {
                type = "leaves",
                attributes = new
                {
                    color = newColor
                },
                relationships = new
                {
                    branch = new
                    {
                        data = new
                        {
                            type = "branches",
                            id = existingBranch.StringId
                        }
                    }
                }
            }
        };

        const string route = "/leaves";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders1 = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
            headers.Add(LeafSignalingDefinition.WaitForResumeSignalHeaderName, "true");
        };

        Task<(HttpResponseMessage, Document)> request1 = _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders1);

        try
        {
            await mediator.WaitForTransactionStartedAsync(TimeSpan.FromSeconds(15));
        }
        catch (TimeoutException)
        {
            // In case the first request never reaches the signaling point, the assertion below displays why it was unable to get there.

            (HttpResponseMessage httpResponseMessage1, _) = await request1;
            httpResponseMessage1.ShouldHaveStatusCode(HttpStatusCode.Created);
        }

        Action<HttpRequestHeaders> setRequestHeaders2 = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders2);

        // Assert
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument2.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument2.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().StartWith($"The request for the provided idempotency key '{idempotencyKey}' is currently being processed.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }
}
