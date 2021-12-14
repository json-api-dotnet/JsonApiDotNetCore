namespace JsonApiDotNetCore.OpenApi;

internal enum JsonApiEndpoint
{
    GetCollection,
    GetSingle,
    GetSecondary,
    GetRelationship,
    Post,
    PostRelationship,
    Patch,
    PatchRelationship,
#pragma warning disable AV1711 // Name members and local functions similarly to members of .NET Framework classes
    Delete,
#pragma warning restore AV1711 // Name members and local functions similarly to members of .NET Framework classes
    DeleteRelationship
}
