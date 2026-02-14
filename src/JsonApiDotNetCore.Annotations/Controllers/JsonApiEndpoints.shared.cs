using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// Lists the built-in JSON:API endpoints, described at https://jsonapi.org/format.
/// </summary>
[PublicAPI]
[Flags]
public enum JsonApiEndpoints
{
    // IMPORTANT: An internal copy of this type exists in the JsonApiDotNetCore.SourceGenerators project.
    // Keep them in sync when making changes.

    /// <summary>
    /// Represents none of the JSON:API endpoints.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents the endpoint to get a collection of primary resources.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// GET /articles HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    GetCollection = 1,

    /// <summary>
    /// Represents the endpoint to get a single primary resource by ID.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// GET /articles/1 HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    GetSingle = 1 << 1,

    /// <summary>
    /// Represents the endpoint to get a secondary resource or collection of secondary resources.
    /// <para>
    /// Example endpoints: <code language="http"><![CDATA[
    /// GET /articles/1/author HTTP/1.1
    /// ]]></code>
    /// <code language="http"><![CDATA[
    /// GET /articles/1/revisions HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    GetSecondary = 1 << 2,

    /// <summary>
    /// Represents the endpoint to get a relationship value.
    /// <para>
    /// Example endpoints: <code language="http"><![CDATA[
    /// GET /articles/1/relationships/author HTTP/1.1
    /// ]]></code>
    /// <code language="http"><![CDATA[
    /// GET /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    GetRelationship = 1 << 3,

    /// <summary>
    /// Represents the endpoint to create a new resource with attributes, relationships, or both.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// POST /articles HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    Post = 1 << 4,

    /// <summary>
    /// Represents the endpoint to add resources to a to-many relationship.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// POST /articles/1/revisions HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    PostRelationship = 1 << 5,

    /// <summary>
    /// Represents the endpoint to update the attributes and/or relationships of an existing resource.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// PATCH /articles/1 HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    Patch = 1 << 6,

    /// <summary>
    /// Represents the endpoint to perform a complete replacement of a relationship on an existing resource.
    /// <para>
    /// Example endpoints: <code language="http"><![CDATA[
    /// PATCH /articles/1/relationships/author HTTP/1.1
    /// ]]></code>
    /// <code language="http"><![CDATA[
    /// PATCH /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    PatchRelationship = 1 << 7,

    /// <summary>
    /// Represents the endpoint to delete an existing resource.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// DELETE /articles/1 HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    Delete = 1 << 8,

    /// <summary>
    /// Represents the endpoint to remove resources from a to-many relationship.
    /// <para>
    /// Example endpoint: <code language="http"><![CDATA[
    /// DELETE /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </para>
    /// </summary>
    DeleteRelationship = 1 << 9,

    /// <summary>
    /// Represents the set of JSON:API endpoints to query resources and relationships.
    /// </summary>
    Query = GetCollection | GetSingle | GetSecondary | GetRelationship,

    /// <summary>
    /// Represents the set of JSON:API endpoints to change resources and relationships.
    /// </summary>
    Command = Post | PostRelationship | Patch | PatchRelationship | Delete | DeleteRelationship,

    /// <summary>
    /// Represents all JSON:API endpoints.
    /// </summary>
    All = Query | Command
}
