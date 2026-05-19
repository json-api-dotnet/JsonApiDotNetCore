using System.Collections.ObjectModel;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

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
        string routeTemplate = resourceType.PublicName;
        return GetEndpointPaths(routeTemplate, resourceType.Relationships);
    }

    public static IReadOnlyDictionary<JsonApiEndpoints, ReadOnlyCollection<string>> GetEndpointPaths(string routeTemplate,
        IReadOnlyCollection<RelationshipAttribute> relationships)
    {
        var endpointToPathMap = new Dictionary<JsonApiEndpoints, List<string>>
        {
            [JsonApiEndpoints.GetCollection] =
            [
                $"paths./{routeTemplate}.get",
                $"paths./{routeTemplate}.head"
            ],
            [JsonApiEndpoints.GetSingle] =
            [
                $"paths./{routeTemplate}/{{id}}.get",
                $"paths./{routeTemplate}/{{id}}.head"
            ],
            [JsonApiEndpoints.GetSecondary] = [],
            [JsonApiEndpoints.GetRelationship] = [],
            [JsonApiEndpoints.Post] = [$"paths./{routeTemplate}.post"],
            [JsonApiEndpoints.PostRelationship] = [],
            [JsonApiEndpoints.Patch] = [$"paths./{routeTemplate}/{{id}}.patch"],
            [JsonApiEndpoints.PatchRelationship] = [],
            [JsonApiEndpoints.Delete] = [$"paths./{routeTemplate}/{{id}}.delete"],
            [JsonApiEndpoints.DeleteRelationship] = []
        };

        foreach (RelationshipAttribute relationship in relationships)
        {
            endpointToPathMap[JsonApiEndpoints.GetSecondary].AddRange([
                $"paths./{routeTemplate}/{{id}}/{relationship.PublicName}.get",
                $"paths./{routeTemplate}/{{id}}/{relationship.PublicName}.head"
            ]);

            endpointToPathMap[JsonApiEndpoints.GetRelationship].AddRange([
                $"paths./{routeTemplate}/{{id}}/relationships/{relationship.PublicName}.get",
                $"paths./{routeTemplate}/{{id}}/relationships/{relationship.PublicName}.head"
            ]);

            endpointToPathMap[JsonApiEndpoints.PatchRelationship].Add($"paths./{routeTemplate}/{{id}}/relationships/{relationship.PublicName}.patch");

            if (relationship is HasManyAttribute)
            {
                endpointToPathMap[JsonApiEndpoints.PostRelationship].Add($"paths./{routeTemplate}/{{id}}/relationships/{relationship.PublicName}.post");
                endpointToPathMap[JsonApiEndpoints.DeleteRelationship].Add($"paths./{routeTemplate}/{{id}}/relationships/{relationship.PublicName}.delete");
            }
        }

        return endpointToPathMap.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly()).AsReadOnly();
    }
}
