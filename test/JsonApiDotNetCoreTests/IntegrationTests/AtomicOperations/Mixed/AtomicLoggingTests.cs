using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        testContext.ConfigureLogging(builder =>
        {
            var loggerProvider = new CapturingLoggerProvider(LogLevel.Information);
            builder.AddProvider(loggerProvider);

            builder.Services.AddSingleton(loggerProvider);
        });

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<OperationsThrowSimulator>();
            services.AddScoped<IOperationsTransactionFactory, ThrowingOperationsTransactionFactory>();
        });
    }

    [Fact]
    public async Task Logs_invalid_request_body_error_at_Information_level()
    {
        // Arrange
        var loggerProvider = _testContext.App.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var throwSimulator = _testContext.App.Services.GetRequiredService<OperationsThrowSimulator>();
        throwSimulator.Reset();

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

        responseDocument.Errors.Should().HaveCount(1);

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();

        logMessages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information &&
            message.Text.Contains("Failed to deserialize request body", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_unhandled_operation_error_at_Error_level()
    {
        // Arrange
        var loggerProvider = _testContext.App.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var throwSimulator = _testContext.App.Services.GetRequiredService<OperationsThrowSimulator>();
        throwSimulator.Reset();
        throwSimulator.ThrowOnOperationStart = true;

        var requestBody = new
        {
            atomic__operations = new[]
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing an operation in this request.");
        error.Detail.Should().Be("Simulated operation failure.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();

        logMessages.Should().ContainSingle(message =>
            message.LogLevel == LogLevel.Error && message.Text.Contains("Simulated operation failure.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_unhandled_error_on_commit_at_Error_level()
    {
        // Arrange
        var loggerProvider = _testContext.App.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var throwSimulator = _testContext.App.Services.GetRequiredService<OperationsThrowSimulator>();
        throwSimulator.Reset();
        throwSimulator.ThrowOnCommit = true;

        var requestBody = new
        {
            atomic__operations = new[]
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Title.Should().Be("An unhandled error occurred while processing this operations request.");
        error.Detail.Should().Be("Simulated commit failure.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations");

        IReadOnlyList<LogMessage> logMessages = loggerProvider.GetMessages();

        logMessages.Should().ContainSingle(message =>
            message.LogLevel == LogLevel.Error && message.Text.Contains("Simulated commit failure.", StringComparison.Ordinal));
    }

    private sealed class OperationsThrowSimulator
    {
        public bool ThrowOnOperationStart { get; set; }
        public bool ThrowOnCommit { get; set; }

        public void Reset()
        {
            ThrowOnOperationStart = false;
            ThrowOnCommit = false;
        }
    }

    private sealed class ThrowingOperationsTransactionFactory(IDbContextResolver dbContextResolver, OperationsThrowSimulator throwSimulator)
        : IOperationsTransactionFactory
    {
        private readonly IDbContextResolver _dbContextResolver = dbContextResolver;
        private readonly OperationsThrowSimulator _throwSimulator = throwSimulator;

        public async Task<TResult> RunInTransactionAsync<TResult>(Func<IOperationsTransaction, Task<TResult>> asyncAction, CancellationToken cancellationToken)
        {
            DbContext dbContext = _dbContextResolver.GetContext();
            IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async _ =>
            {
                await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                await using var operationsTransaction = new ThrowingOperationsTransaction(_throwSimulator, transaction, dbContext);

                TResult result = await asyncAction(operationsTransaction);

                await operationsTransaction.CommitAsync(cancellationToken);

                return result;
            }, cancellationToken);
        }

        private sealed class ThrowingOperationsTransaction(OperationsThrowSimulator throwSimulator, IDbContextTransaction transaction, DbContext dbContext)
            : IOperationsTransaction
        {
            private readonly OperationsThrowSimulator _throwSimulator = throwSimulator;
            private readonly IDbContextTransaction _transaction = transaction;
            private readonly DbContext _dbContext = dbContext;

            public string TransactionId => _transaction.TransactionId.ToString();

            public Task BeforeProcessOperationAsync(CancellationToken cancellationToken)
            {
                if (_throwSimulator.ThrowOnOperationStart)
                {
                    throw new InvalidOperationException("Simulated operation failure.");
                }

                _dbContext.ResetChangeTracker();
                return Task.CompletedTask;
            }

            public Task AfterProcessOperationAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                if (_throwSimulator.ThrowOnCommit)
                {
                    throw new InvalidOperationException("Simulated commit failure.");
                }

                return _transaction.CommitAsync(cancellationToken);
            }

            public ValueTask DisposeAsync()
            {
                return _transaction.DisposeAsync();
            }
        }
    }
}
