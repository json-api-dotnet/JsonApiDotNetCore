using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    public sealed class ExceptionHandlerTests : IClassFixture<IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> _testContext;

        public ExceptionHandlerTests(IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<ThrowingArticlesController>();
            testContext.UseController<ConsumerArticlesController>();

            var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);

            testContext.ConfigureLogging(options =>
            {
                options.ClearProviders();
                options.AddProvider(loggerFactory);
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                services.AddSingleton(loggerFactory);
            });

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceService<ConsumerArticleService>();
                services.AddScoped<IExceptionHandler, AlternateExceptionHandler>();
            });
        }

        [Fact]
        public async Task Logs_and_produces_error_response_for_custom_exception()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Gone);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Gone);
            error.Title.Should().Be("The requested article is no longer available.");
            error.Detail.Should().Be("Article with code 'X123' is no longer available.");
            ((JsonElement)error.Meta["support"]).GetString().Should().Be("Please contact us for info about similar articles at company@email.com.");

            loggerFactory.Logger.Messages.Should().HaveCount(1);
            loggerFactory.Logger.Messages.Single().LogLevel.Should().Be(LogLevel.Warning);
            loggerFactory.Logger.Messages.Single().Text.Should().Contain("Article with code 'X123' is no longer available.");
        }

        [Fact]
        public async Task Logs_and_produces_error_response_on_serialization_failure()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing this request.");
            error.Detail.Should().Be("Exception has been thrown by the target of an invocation.");

            IEnumerable<string> stackTraceLines = ((JsonElement)error.Meta["stackTrace"]).EnumerateArray().Select(token => token.GetString());
            stackTraceLines.Should().ContainMatch("* System.InvalidOperationException: Article status could not be determined.*");

            loggerFactory.Logger.Messages.Should().HaveCount(1);
            loggerFactory.Logger.Messages.Single().LogLevel.Should().Be(LogLevel.Error);
            loggerFactory.Logger.Messages.Single().Text.Should().Contain("Exception has been thrown by the target of an invocation.");
        }
    }
}
