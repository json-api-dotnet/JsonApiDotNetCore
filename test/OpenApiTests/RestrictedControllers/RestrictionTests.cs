using System.Collections.ObjectModel;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.RestrictedControllers;

public sealed class RestrictionTests : IClassFixture<OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;

    public RestrictionTests(OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<DataStreamController>();
        testContext.UseController<ReadOnlyChannelsController>();
        testContext.UseController<WriteOnlyChannelsController>();
        testContext.UseController<RelationshipChannelsController>();
        testContext.UseController<ReadOnlyResourceChannelsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData(typeof(DataStream), JsonApiEndpoints.GetCollection | JsonApiEndpoints.GetSingle)]
    [InlineData(typeof(ReadOnlyChannel), ReadOnlyChannel.ControllerEndpoints)]
    [InlineData(typeof(WriteOnlyChannel), WriteOnlyChannel.ControllerEndpoints)]
    [InlineData(typeof(RelationshipChannel), RelationshipChannel.ControllerEndpoints)]
    [InlineData(typeof(ReadOnlyResourceChannel), ReadOnlyResourceChannel.ControllerEndpoints)]
    public async Task Only_expected_endpoints_are_exposed(Type resourceClrType, JsonApiEndpoints expected)
    {
        // Arrange
        var resourceGraph = _testContext.Factory.Services.GetRequiredService<IResourceGraph>();
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);
        IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> endpointToPathMap = JsonPathBuilder.GetEndpointPaths(resourceType);

        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string[] pathsExpected = JsonPathBuilder.KnownEndpoints.Where(endpoint => expected.HasFlag(endpoint))
            .SelectMany(endpoint => endpointToPathMap[endpoint]).ToArray();

        string[] pathsNotExpected = endpointToPathMap.Values.SelectMany(paths => paths).Except(pathsExpected).ToArray();

        foreach (string path in pathsExpected)
        {
            document.Should().ContainPath(path);
        }

        foreach (string path in pathsNotExpected)
        {
            document.Should().NotContainPath(path);
        }
    }
}
