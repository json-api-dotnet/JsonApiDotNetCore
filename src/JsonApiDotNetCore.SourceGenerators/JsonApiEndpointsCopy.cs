namespace JsonApiDotNetCore.SourceGenerators;

// IMPORTANT: A copy of this type exists in the JsonApiDotNetCore project. Keep these in sync when making changes.
[Flags]
public enum JsonApiEndpointsCopy
{
    /// <summary>
    /// Represents none of the JSON:API endpoints.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents the endpoint to get a collection of primary resources. Example: <code><![CDATA[
    /// GET /articles HTTP/1.1
    /// ]]></code>
    /// </summary>
    GetCollection = 1,

    /// <summary>
    /// Represents the endpoint to get a single primary resource by ID. Example: <code><![CDATA[
    /// GET /articles/1 HTTP/1.1
    /// ]]></code>
    /// </summary>
    GetSingle = 1 << 1,

    /// <summary>
    /// Represents the endpoint to get a secondary resource or collection of secondary resources. Example:
    /// <code><![CDATA[
    /// GET /articles/1/author HTTP/1.1
    /// ]]></code> Example: <code><![CDATA[
    /// GET /articles/1/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    GetSecondary = 1 << 2,

    /// <summary>
    /// Represents the endpoint to get a relationship value. Example: <code><![CDATA[
    /// GET /articles/1/relationships/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// GET /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    GetRelationship = 1 << 3,

    /// <summary>
    /// Represents the endpoint to create a new resource with attributes, relationships or both. Example:
    /// <code><![CDATA[
    /// POST /articles HTTP/1.1
    /// ]]></code>
    /// </summary>
    Post = 1 << 4,

    /// <summary>
    /// Represents the endpoint to add resources to a to-many relationship. Example: <code><![CDATA[
    /// POST /articles/1/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    PostRelationship = 1 << 5,

    /// <summary>
    /// Represents the endpoint to update the attributes and/or relationships of an existing resource. Example:
    /// <code><![CDATA[
    /// PATCH /articles/1
    /// ]]></code>
    /// </summary>
    Patch = 1 << 6,

    /// <summary>
    /// Represents the endpoint to perform a complete replacement of a relationship on an existing resource. Example:
    /// <code><![CDATA[
    /// PATCH /articles/1/relationships/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// PATCH /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    PatchRelationship = 1 << 7,

    /// <summary>
    /// Represents the endpoint to delete an existing resource. Example: <code><![CDATA[
    /// DELETE /articles/1
    /// ]]></code>
    /// </summary>
    Delete = 1 << 8,

    /// <summary>
    /// Represents the endpoint to remove resources from a to-many relationship. Example:
    /// <code><![CDATA[
    /// DELETE /articles/1/relationships/revisions
    /// ]]></code>
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
    /// Represents all of the JSON:API endpoints.
    /// </summary>
    All = Query | Command
}
