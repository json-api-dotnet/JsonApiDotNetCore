using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

public sealed class AtomicTransactionConsistencyTests
    : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>, IAsyncLifetime
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicTransactionConsistencyTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddResourceRepository<PerformerRepository>();
            services.AddResourceRepository<MusicTrackRepository>();
            services.AddResourceRepository<LyricRepository>();

            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";

            string dbConnectionString =
                $"Host=localhost;Database=JsonApiTest-Extra-{Guid.NewGuid():N};User ID=postgres;Password={postgresPassword};Include Error Detail=true";

            services.AddDbContext<ExtraDbContext>(options => options.UseNpgsql(dbConnectionString));
        });
    }

    [Fact]
    public async Task Cannot_use_non_transactional_repository()
    {
        // Arrange
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported resource type in atomic:operations request.");
        error.Detail.Should().Be("Operations on resources of type 'performers' cannot be used because transaction support is unavailable.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_use_transactional_repository_without_active_transaction()
    {
        // Arrange
        string newTrackTitle = _fakers.MusicTrack.Generate().Title;

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "musicTracks",
                        attributes = new
                        {
                            title = newTrackTitle
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
        error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_use_distributed_transaction()
    {
        // Arrange
        string newLyricText = _fakers.Lyric.Generate().Text;

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "lyrics",
                        attributes = new
                        {
                            text = newLyricText
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Unsupported combination of resource types in atomic:operations request.");
        error.Detail.Should().Be("All operations need to participate in a single shared transaction, which is not the case for this request.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return DeleteExtraDatabaseAsync();
    }

    private async Task DeleteExtraDatabaseAsync()
    {
        await using AsyncServiceScope scope = _testContext.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExtraDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
    }
}
