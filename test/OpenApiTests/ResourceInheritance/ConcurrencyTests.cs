using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ResourceInheritance;

public sealed class ConcurrencyTests : ResourceInheritanceTests
{
    private readonly OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public ConcurrencyTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
        : base(testContext, testOutputHelper, true, false)
    {
        _testContext = testContext;

        testContext.ConfigureServices(services => services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders()));
    }

    [Fact]
    public async Task Can_download_OpenAPI_documents_in_parallel()
    {
        // Arrange
        const int count = 15;
        var downloadTasks = new Task[count];

        for (int index = 0; index < count; index++)
        {
            downloadTasks[index] = _testContext.CreateSwaggerDocumentAsync();
        }

        // Act
        Func<Task> action = async () => await Task.WhenAll(downloadTasks);

        // Assert
        await action.Should().NotThrowAsync();
    }
}
