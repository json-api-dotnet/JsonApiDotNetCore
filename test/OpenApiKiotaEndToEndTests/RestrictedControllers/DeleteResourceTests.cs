using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.RestrictedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.RestrictedControllers;

public sealed class DeleteResourceTests : IClassFixture<IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly RestrictionFakers _fakers = new();

    public DeleteResourceTests(IntegrationTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<WriteOnlyChannelsController>();
    }

    [Fact]
    public async Task Can_delete_existing_resource()
    {
        // Arrange
        WriteOnlyChannel existingChannel = _fakers.WriteOnlyChannel.GenerateOne();
        existingChannel.VideoStream = _fakers.DataStream.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WriteOnlyChannels.Add(existingChannel);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        await apiClient.WriteOnlyChannels[existingChannel.StringId].DeleteAsync();

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WriteOnlyChannel? channelInDatabase = await dbContext.WriteOnlyChannels.FirstWithIdOrDefaultAsync(existingChannel.Id);

            channelInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_unknown_resource()
    {
        // Arrange
        string unknownChannelId = Unknown.StringId.For<WriteOnlyChannel, long>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new RestrictedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.WriteOnlyChannels[unknownChannelId].DeleteAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'writeOnlyChannels' with ID '{unknownChannelId}' does not exist.");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
