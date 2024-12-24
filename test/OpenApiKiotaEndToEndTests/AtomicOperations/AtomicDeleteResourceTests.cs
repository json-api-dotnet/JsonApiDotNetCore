using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode;
using OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.AtomicOperations;

public sealed class AtomicDeleteResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly OperationsFakers _fakers;

    public AtomicDeleteResourceTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<OperationsController>();

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new AtomicOperationsClient(requestAdapter);

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                new DeleteEnrollmentOperation
                {
                    Op = RemoveOperationCode.Remove,
                    Ref = new EnrollmentIdentifierInRequest
                    {
                        Type = EnrollmentResourceType.Enrollments,
                        Id = existingEnrollment.StringId!
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Enrollment? enrollmentInDatabase = await dbContext.Enrollments.FirstWithIdOrDefaultAsync(existingEnrollment.Id);

            enrollmentInDatabase.Should().BeNull();
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
