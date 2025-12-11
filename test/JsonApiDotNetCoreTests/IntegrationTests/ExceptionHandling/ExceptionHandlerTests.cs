using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

public sealed class ExceptionHandlerTests : IClassFixture<IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> _testContext;

    public ExceptionHandlerTests(IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ThrowingArticlesController>();
        testContext.UseController<ConsumerArticlesController>();

        testContext.ConfigureLogging(builder =>
        {
            var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
            builder.AddProvider(loggerProvider);

            builder.Services.AddSingleton(loggerProvider);
        });

        testContext.ConfigureServices(services =>
        {
            services.AddResourceService<ConsumerArticleService>();

            services.AddScoped<IExceptionHandler, AlternateExceptionHandler>();
        });
    }

    [Fact]
    public async Task Logs_and_produces_error_response_for_custom_exception()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var consumerArticle = new ConsumerArticle
        {
            Code = $"{ConsumerArticleService.UnavailableArticlePrefix}123"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ConsumerArticles.Add(consumerArticle);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/consumerArticles/{consumerArticle.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Gone);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Gone);
        error.Title.Should().Be("The requested article is no longer available.");
        error.Detail.Should().Be("Article with code 'X123' is no longer available.");

        error.Meta.Should().ContainKey("support").WhoseValue.With(value =>
        {
            JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
            element.GetString().Should().Be("Please contact us for info about similar articles at company@email.com.");
        });

        responseDocument.Meta.Should().BeNull();

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();
        logMessages.Should().HaveCount(1);

        logMessages[0].LogLevel.Should().Be(LogLevel.Warning);
        logMessages[0].Text.Should().Contain("Article with code 'X123' is no longer available.");
    }

    [Fact]
    public async Task Logs_and_produces_error_response_on_deserialization_failure()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        const string requestBody = """{ "data": { "type": "" } }""";

        const string route = "/consumerArticles";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be("Resource type '' does not exist.");
        error.Meta.Should().ContainRequestBody(requestBody);
        error.Meta.Should().HaveStackTrace();

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();
        logMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task Logs_and_produces_error_response_on_serialization_failure()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var throwingArticle = new ThrowingArticle();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ThrowingArticles.Add(throwingArticle);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/throwingArticles/{throwingArticle.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing this request.");
        error.Detail.Should().Be("Exception has been thrown by the target of an invocation.");
        error.Meta.Should().HaveInStackTrace("*ThrowingArticle*");

        responseDocument.Meta.Should().BeNull();

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();
        logMessages.Should().HaveCount(1);

        logMessages[0].LogLevel.Should().Be(LogLevel.Error);
        logMessages[0].Text.Should().Contain("Exception has been thrown by the target of an invocation.");
    }
}
