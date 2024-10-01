using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Mixed;

public sealed class AtomicLoggingTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;

    public AtomicLoggingTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureLogging(options =>
        {
            var loggerProvider = new CapturingLoggerProvider(LogLevel.Information);
            options.AddProvider(loggerProvider);
            options.SetMinimumLevel(LogLevel.Information);

            options.Services.AddSingleton(loggerProvider);
        });

        testContext.ConfigureServices(services => services.AddSingleton<IOperationsTransactionFactory, ThrowingOperationsTransactionFactory>());
    }

    [Fact]
    public async Task Logs_unhandled_exception_at_Error_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var transactionFactory = (ThrowingOperationsTransactionFactory)_testContext.Factory.Services.GetRequiredService<IOperationsTransactionFactory>();
        transactionFactory.ThrowOnOperationStart = true;

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "performers",
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing an operation in this request.");
        error.Detail.Should().Be("Simulated failure.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();

        logMessages.Should().ContainSingle(message =>
            message.LogLevel == LogLevel.Error && message.Text.Contains("Simulated failure.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_invalid_request_body_error_at_Information_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var transactionFactory = (ThrowingOperationsTransactionFactory)_testContext.Factory.Services.GetRequiredService<IOperationsTransactionFactory>();
        transactionFactory.ThrowOnOperationStart = false;

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "update"
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();

        logMessages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information &&
            message.Text.Contains("Failed to deserialize request body", StringComparison.Ordinal));
    }

    private sealed class ThrowingOperationsTransactionFactory : IOperationsTransactionFactory
    {
        public bool ThrowOnOperationStart { get; set; }

        public Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            IOperationsTransaction transaction = new ThrowingOperationsTransaction(this);
            return Task.FromResult(transaction);
        }

        private sealed class ThrowingOperationsTransaction(ThrowingOperationsTransactionFactory owner) : IOperationsTransaction
        {
            private readonly ThrowingOperationsTransactionFactory _owner = owner;

            public string TransactionId => "some";

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }

            public Task BeforeProcessOperationAsync(CancellationToken cancellationToken)
            {
                return ThrowIfEnabled();
            }

            public Task AfterProcessOperationAsync(CancellationToken cancellationToken)
            {
                return ThrowIfEnabled();
            }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                return ThrowIfEnabled();
            }

            private Task ThrowIfEnabled()
            {
                if (_owner.ThrowOnOperationStart)
                {
                    throw new InvalidOperationException("Simulated failure.");
                }

                return Task.CompletedTask;
            }
        }
    }
}
