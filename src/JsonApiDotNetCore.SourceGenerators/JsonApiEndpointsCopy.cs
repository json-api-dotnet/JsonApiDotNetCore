using System;

namespace JsonApiDotNetCore.SourceGenerators
{
    // IMPORTANT: A copy of this type exists in the JsonApiDotNetCore project. Keep these in sync when making changes.
    [Flags]
    public enum JsonApiEndpointsCopy
    {
        None = 0,
        GetCollection = 1,
        GetSingle = 1 << 1,
        GetSecondary = 1 << 2,
        GetRelationship = 1 << 3,
        Post = 1 << 4,
        PostRelationship = 1 << 5,
        Patch = 1 << 6,
        PatchRelationship = 1 << 7,
        Delete = 1 << 8,
        DeleteRelationship = 1 << 9,

        Query = GetCollection | GetSingle | GetSecondary | GetRelationship,
        Command = Post | PostRelationship | Patch | PatchRelationship | Delete | DeleteRelationship,

        All = Query | Command
    }
}
