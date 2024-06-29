namespace JsonApiDotNetCore.OpenApi;

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
    DeleteRelationship
}
