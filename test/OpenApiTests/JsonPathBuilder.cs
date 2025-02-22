using System.Collections.ObjectModel;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace OpenApiTests;

internal static class JsonPathBuilder
{
    public static readonly IReadOnlyCollection<JsonApiEndpoints> KnownEndpoints =
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

    public static IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> GetEndpointPaths(ResourceType resourceType)
    {
        var endpointToPathMap = new Dictionary<JsonApiEndpoints, List<string>>
        {
            [JsonApiEndpoints.GetCollection] =
            [
                $"paths./{resourceType.PublicName}.get",
                $"paths./{resourceType.PublicName}.head"
            ],
            [JsonApiEndpoints.GetSingle] =
            [
                $"paths./{resourceType.PublicName}/{{id}}.get",
                $"paths./{resourceType.PublicName}/{{id}}.head"
            ],
            [JsonApiEndpoints.GetSecondary] = [],
            [JsonApiEndpoints.GetRelationship] = [],
            [JsonApiEndpoints.Post] = [$"paths./{resourceType.PublicName}.post"],
            [JsonApiEndpoints.PostRelationship] = [],
            [JsonApiEndpoints.Patch] = [$"paths./{resourceType.PublicName}/{{id}}.patch"],
            [JsonApiEndpoints.PatchRelationship] = [],
            [JsonApiEndpoints.Delete] = [$"paths./{resourceType.PublicName}/{{id}}.delete"],
            [JsonApiEndpoints.DeleteRelationship] = []
        };

        foreach (RelationshipAttribute relationship in resourceType.Relationships)
        {
            endpointToPathMap[JsonApiEndpoints.GetSecondary].AddRange([
                $"paths./{resourceType.PublicName}/{{id}}/{relationship.PublicName}.get",
                $"paths./{resourceType.PublicName}/{{id}}/{relationship.PublicName}.head"
            ]);

            endpointToPathMap[JsonApiEndpoints.GetRelationship].AddRange([
                $"paths./{resourceType.PublicName}/{{id}}/relationships/{relationship.PublicName}.get",
                $"paths./{resourceType.PublicName}/{{id}}/relationships/{relationship.PublicName}.head"
            ]);

            endpointToPathMap[JsonApiEndpoints.PatchRelationship].Add($"paths./{resourceType.PublicName}/{{id}}/relationships/{relationship.PublicName}.patch");

            if (relationship is HasManyAttribute)
            {
                endpointToPathMap[JsonApiEndpoints.PostRelationship].Add(
                    $"paths./{resourceType.PublicName}/{{id}}/relationships/{relationship.PublicName}.post");

                endpointToPathMap[JsonApiEndpoints.DeleteRelationship].Add(
                    $"paths./{resourceType.PublicName}/{{id}}/relationships/{relationship.PublicName}.delete");
            }
        }

        return endpointToPathMap.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly()).AsReadOnly();
    }
}
