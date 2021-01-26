using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ExceptionHandlerTests
        : IClassFixture<IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> _testContext;

        public ExceptionHandlerTests(IntegrationTestContext<TestableStartup<ErrorDbContext>, ErrorDbContext> testContext)
        {
            _testContext = testContext;

            FakeLoggerFactory loggerFactory = null;

            testContext.ConfigureLogging(options =>
            {
                loggerFactory = new FakeLoggerFactory();

                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Warning);
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                if (loggerFactory != null)
                {
                    services.AddSingleton(_ => loggerFactory);
                }
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
                Code = "X123"
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.ConsumerArticles.Add(consumerArticle);
                await dbContext.SaveChangesAsync();
            });

            var route = "/consumerArticles/" + consumerArticle.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Gone);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Gone);
            responseDocument.Errors[0].Title.Should().Be("The requested article is no longer available.");
            responseDocument.Errors[0].Detail.Should().Be("Article with code 'X123' is no longer available.");
            responseDocument.Errors[0].Meta.Data["support"].Should().Be("Please contact us for info about similar articles at company@email.com.");

            loggerFactory.Logger.Messages.Should().HaveCount(1);
            loggerFactory.Logger.Messages.Single().Text.Should().Contain("Article with code 'X123' is no longer available.");
        }
    }
}
