using JetBrains.Annotations;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Controllers;

// IMPORTANT: An internal copy of this type exists in the SourceGenerators project. Keep these in sync when making changes.
[PublicAPI]
[Flags]
public enum JsonApiEndpoints
{
    None = 0,

    /// <summary>
    /// Endpoint to get a collection of primary resources.
    /// </summary>
    /// <example>
    /// <code><![CDATA[GET /articles]]></code>
    /// </example>
    GetCollection = 1,

    /// <summary>
    /// Endpoint to get a single primary resource by ID.
    /// </summary>
    /// <example>
    /// <code><![CDATA[GET /articles/1]]></code>
    /// </example>
    GetSingle = 1 << 1,

    /// <summary>
    /// Endpoint to get a secondary resource or collection of secondary resources.
    /// </summary>
    /// <example>
    /// <code><![CDATA[GET /articles/1/author]]></code>
    /// </example>
    GetSecondary = 1 << 2,

    /// <summary>
    /// Endpoint to get a relationship value, which can be a <c>null</c>, a single object or a collection.
    /// </summary>
    /// <example>
    /// <code><![CDATA[GET /articles/1/relationships/author]]></code>
    /// </example>
    /// <example>
    /// <code><![CDATA[GET /articles/1/relationships/revisions]]></code>
    /// </example>
    GetRelationship = 1 << 3,

    /// <summary>
    /// Endpoint to creates a new resource with attributes, relationships or both.
    /// </summary>
    /// <example>
    /// <code><![CDATA[POST /articles]]></code>
    /// </example>
    Post = 1 << 4,

    /// <summary>
    /// Endpoint to add resources to a to-many relationship.
    /// </summary>
    /// <example>
    /// <code><![CDATA[POST /articles/1/revisions]]></code>
    /// </example>
    PostRelationship = 1 << 5,

    /// <summary>
    /// Endpoint to update the attributes and/or relationships of an existing resource.
    /// </summary>
    /// <example>
    /// <code><![CDATA[PATCH /articles/1]]></code>
    /// </example>
    Patch = 1 << 6,

    /// <summary>
    /// Endpoint to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <example>
    /// <code><![CDATA[PATCH /articles/1/relationships/author]]></code>
    /// </example>
    /// <example>
    /// <code><![CDATA[PATCH /articles/1/relationships/revisions]]></code>
    /// </example>
    PatchRelationship = 1 << 7,

    /// <summary>
    /// Endpoint to delete an existing resource.
    /// </summary>
    /// <example>
    /// <code><![CDATA[DELETE /articles/1]]></code>
    /// </example>
    Delete = 1 << 8,

    /// <summary>
    /// Endpoint to remove resources from a to-many relationship.
    /// </summary>
    /// <example>
    /// <code><![CDATA[DELETE /articles/1/relationships/revisions]]></code>
    /// </example>
    DeleteRelationship = 1 << 9,

    /// <summary>
    /// All read-only endpoints.
    /// </summary>
    Query = GetCollection | GetSingle | GetSecondary | GetRelationship,

    /// <summary>
    /// All write endpoints.
    /// </summary>
    Command = Post | PostRelationship | Patch | PatchRelationship | Delete | DeleteRelationship,

    /// <summary>
    /// All endpoints.
    /// </summary>
    All = Query | Command
}
