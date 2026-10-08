using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

public sealed class AtomicRetryTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicRetryTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureDbContextOptions(options => options.EnableRetryOnFailure());

        testContext.ConfigureServices(services => services.AddSingleton<OperationsTransientFailureSimulator>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_process_operations_with_retrying_execution_strategy(bool simulateFailureOnFirstSaveChanges)
    {
        // Arrange
        var failureSimulator = _testContext.App.Services.GetRequiredService<OperationsTransientFailureSimulator>();
        failureSimulator.Reset();

        if (simulateFailureOnFirstSaveChanges)
        {
            failureSimulator.FailOnNextAttempt = true;
            failureSimulator.UseTransientError = true;
        }

        string newArtistName = _fakers.Performer.GenerateOne().ArtistName!;
        DateTimeOffset newBornAt = _fakers.Performer.GenerateOne().BornAt;
        const string newPerformerLocalId = "new-performer";

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
                        lid = newPerformerLocalId,
                        attributes = new
                        {
                            artistName = newArtistName,
                            bornAt = newBornAt
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("performers");
            resource.Attributes.Should().NotBeNull();
            resource.Attributes!["artistName"].Should().Be(newArtistName);
        });

        int newPerformerId = int.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Performer? performerInDatabase = await dbContext.Performers.FirstWithIdOrDefaultAsync(newPerformerId);

            performerInDatabase.Should().NotBeNull();
            performerInDatabase.ArtistName.Should().Be(newArtistName);
        });

        failureSimulator.AttemptCount.Should().Be(simulateFailureOnFirstSaveChanges ? 2 : 1);
    }

    [Fact]
    public async Task Does_not_retry_on_non_transient_failure()
    {
        // Arrange
        var failureSimulator = _testContext.App.Services.GetRequiredService<OperationsTransientFailureSimulator>();
        failureSimulator.Reset();
        failureSimulator.FailOnNextAttempt = true;

        string newArtistName = _fakers.Performer.GenerateOne().ArtistName!;
        DateTimeOffset newBornAt = _fakers.Performer.GenerateOne().BornAt;

        await _testContext.RunOnDatabaseAsync(async dbContext => await dbContext.ClearTableAsync<Performer>());

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
                            artistName = newArtistName,
                            bornAt = newBornAt
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
        error.Detail.Should().Be("Failed to persist changes in the underlying data store.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<Performer> performersInDatabase = await dbContext.Performers.ToListAsync();

            performersInDatabase.Should().BeEmpty();
        });

        failureSimulator.AttemptCount.Should().Be(1);
    }
}
