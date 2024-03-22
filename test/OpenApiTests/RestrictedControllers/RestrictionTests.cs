using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Controllers;
using TestBuildingBlocks;
using Xunit;

#pragma warning disable AV1532 // Loop statement contains nested loop

namespace OpenApiTests.RestrictedControllers;

public sealed class RestrictionTests : IClassFixture<OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext>>
{
    private static readonly JsonApiEndpoints[] KnownEndpoints =
    [
        JsonApiEndpoints.GetCollection,
        JsonApiEndpoints.GetSingle,
        JsonApiEndpoints.GetSecondary,
        JsonApiEndpoints.GetRelationship,
        JsonApiEndpoints.Post,
        JsonApiEndpoints.PostRelationship,
        JsonApiEndpoints.Patch,
        JsonApiEndpoints.PatchRelationship,
        JsonApiEndpoints.Delete,
        JsonApiEndpoints.DeleteRelationship
    ];

    private readonly OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;

    public RestrictionTests(OpenApiTestContext<OpenApiStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DataStreamController>();
        testContext.UseController<ReadOnlyChannelsController>();
        testContext.UseController<WriteOnlyChannelsController>();
        testContext.UseController<RelationshipChannelsController>();
        testContext.UseController<ReadOnlyResourceChannelsController>();

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
        string resourceName = resourceClrType.Name.Camelize().Pluralize();

        var endpointToPathMap = new Dictionary<JsonApiEndpoints, string[]>
        {
            [JsonApiEndpoints.GetCollection] =
            [
                $"/{resourceName}.get",
                $"/{resourceName}.head"
            ],
            [JsonApiEndpoints.GetSingle] =
            [
                $"/{resourceName}/{{id}}.get",
                $"/{resourceName}/{{id}}.head"
            ],
            [JsonApiEndpoints.GetSecondary] =
            [
                $"/{resourceName}/{{id}}/audioStreams.get",
                $"/{resourceName}/{{id}}/audioStreams.head",
                $"/{resourceName}/{{id}}/ultraHighDefinitionVideoStream.get",
                $"/{resourceName}/{{id}}/ultraHighDefinitionVideoStream.head",
                $"/{resourceName}/{{id}}/videoStream.get",
                $"/{resourceName}/{{id}}/videoStream.head"
            ],
            [JsonApiEndpoints.GetRelationship] =
            [
                $"/{resourceName}/{{id}}/relationships/audioStreams.get",
                $"/{resourceName}/{{id}}/relationships/audioStreams.head",
                $"/{resourceName}/{{id}}/relationships/ultraHighDefinitionVideoStream.get",
                $"/{resourceName}/{{id}}/relationships/ultraHighDefinitionVideoStream.head",
                $"/{resourceName}/{{id}}/relationships/videoStream.get",
                $"/{resourceName}/{{id}}/relationships/videoStream.head"
            ],
            [JsonApiEndpoints.Post] = [$"/{resourceName}.post"],
            [JsonApiEndpoints.PostRelationship] = [$"/{resourceName}/{{id}}/relationships/audioStreams.post"],
            [JsonApiEndpoints.Patch] = [$"/{resourceName}/{{id}}.patch"],
            [JsonApiEndpoints.PatchRelationship] =
            [
                $"/{resourceName}/{{id}}/relationships/audioStreams.patch",
                $"/{resourceName}/{{id}}/relationships/ultraHighDefinitionVideoStream.patch",
                $"/{resourceName}/{{id}}/relationships/videoStream.patch"
            ],
            [JsonApiEndpoints.Delete] = [$"/{resourceName}/{{id}}.delete"],
            [JsonApiEndpoints.DeleteRelationship] = [$"/{resourceName}/{{id}}/relationships/audioStreams.delete"]
        };

        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        foreach (JsonApiEndpoints endpoint in KnownEndpoints.Where(value => expected.HasFlag(value)))
        {
            string[] pathsExpected = endpointToPathMap[endpoint];
            string[] pathsNotExpected = endpointToPathMap.Values.SelectMany(paths => paths).Except(pathsExpected).ToArray();

            // Assert
            foreach (string path in pathsExpected)
            {
                document.Should().ContainPath($"paths.{path}");
            }

            foreach (string path in pathsNotExpected)
            {
                document.Should().NotContainPath($"paths{path}");
            }
        }
    }
}
