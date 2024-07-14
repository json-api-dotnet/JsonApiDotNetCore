namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal enum JsonApiEndpoint
{
    GetCollection,
    GetSingle,
    GetSecondary,
    GetRelationship,
    PostResource,
    PostRelationship,
    PatchResource,
    PatchRelationship,
    DeleteResource,
    DeleteRelationship,
    PostOperations
}
