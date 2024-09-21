using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class AtomicDeleteResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly OperationsFakers _fakers;

    public AtomicDeleteResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<OperationsController>();

        testContext.ConfigureServices(services => services.AddSingleton<ISystemClock, FrozenSystemClock>());

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        Enrollment existingEnrollment = _fakers.Enrollment.GenerateOne();
        existingEnrollment.Student = _fakers.Student.GenerateOne();
        existingEnrollment.Course = _fakers.Course.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Enrollments.Add(existingEnrollment);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new AtomicOperationsClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new DeleteEnrollmentOperation
                {
                    Ref = new EnrollmentIdentifierInRequest
                    {
                        Id = existingEnrollment.StringId!
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Enrollment? enrollmentInDatabase = await dbContext.Enrollments.FirstWithIdOrDefaultAsync(existingEnrollment.Id);

            enrollmentInDatabase.Should().BeNull();
        });
    }
}
