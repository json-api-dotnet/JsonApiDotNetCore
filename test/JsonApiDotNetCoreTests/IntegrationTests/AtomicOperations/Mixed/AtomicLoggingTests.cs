#nullable disable

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Mixed
{
    public sealed class AtomicLoggingTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;

        public AtomicLoggingTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            var loggerFactory = new FakeLoggerFactory(LogLevel.Information);

            testContext.ConfigureLogging(options =>
            {
                options.ClearProviders();
                options.AddProvider(loggerFactory);
                options.SetMinimumLevel(LogLevel.Information);
            });

            testContext.ConfigureServicesBeforeStartup(services =>
            {
                services.AddSingleton(loggerFactory);
            });

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddSingleton<IOperationsTransactionFactory, ThrowingOperationsTransactionFactory>();
            });
        }

        [Fact]
        public async Task Logs_at_error_level_on_unhandled_exception()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.InternalServerError);

            responseDocument.Errors.Should().HaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.Title.Should().Be("An unhandled error occurred while processing an operation in this request.");
            error.Detail.Should().Be("Simulated failure.");
            error.Source.Pointer.Should().Be("/atomic:operations[0]");

            loggerFactory.Logger.Messages.Should().NotBeEmpty();

            loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Error &&
                message.Text.Contains("Simulated failure.", StringComparison.Ordinal));
        }

        [Fact]
        public async Task Logs_at_info_level_on_invalid_request_body()
        {
            // Arrange
            var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
            loggerFactory.Logger.Clear();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);

            loggerFactory.Logger.Messages.Should().NotBeEmpty();

            loggerFactory.Logger.Messages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information &&
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

            private sealed class ThrowingOperationsTransaction : IOperationsTransaction
            {
                private readonly ThrowingOperationsTransactionFactory _owner;

                public string TransactionId => "some";

                public ThrowingOperationsTransaction(ThrowingOperationsTransactionFactory owner)
                {
                    _owner = owner;
                }

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
                        throw new Exception("Simulated failure.");
                    }

                    return Task.CompletedTask;
                }
            }
        }
    }
}
