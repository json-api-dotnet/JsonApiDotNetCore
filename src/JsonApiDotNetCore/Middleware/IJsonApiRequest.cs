using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Metadata associated with the JSON:API request that is currently being processed.
/// </summary>
public interface IJsonApiRequest
{
    /// <summary>
    /// Routing information, based on the path of the request URL.
    /// </summary>
    public EndpointKind Kind { get; }

    /// <summary>
    /// The ID of the primary resource for this request. This would be <c>null</c> in "/blogs", "123" in "/blogs/123" or "/blogs/123/author". This is
    /// <c>null</c> before and after processing operations in an atomic:operations request.
    /// </summary>
    string? PrimaryId { get; }

    /// <summary>
    /// The primary resource type for this request. This would be "blogs" in "/blogs", "/blogs/123" or "/blogs/123/author". This is <c>null</c> before and
    /// after processing operations in an atomic:operations request.
    /// </summary>
    ResourceType? PrimaryResourceType { get; }

    /// <summary>
    /// The secondary resource type for this request. This would be <c>null</c> in "/blogs", "/blogs/123" and "/blogs/123/unknownResource" or "people" in
    /// "/blogs/123/author" and "/blogs/123/relationships/author". This is <c>null</c> before and after processing operations in an atomic:operations
    /// request.
    /// </summary>
    ResourceType? SecondaryResourceType { get; }

    /// <summary>
    /// The relationship for this request. This would be <c>null</c> in "/blogs", "/blogs/123" and "/blogs/123/unknownResource" or "author" in
    /// "/blogs/123/author" and "/blogs/123/relationships/author". This is <c>null</c> before and after processing operations in an atomic:operations
    /// request.
    /// </summary>
    RelationshipAttribute? Relationship { get; }

    /// <summary>
    /// Indicates whether this request targets a single resource or a collection of resources.
    /// </summary>
    bool IsCollection { get; }

    /// <summary>
    /// Indicates whether this request targets only fetching of data (resources and relationships), as opposed to applying changes.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// In case of a non-readonly request, this indicates the kind of write operation currently being processed. This is <c>null</c> when processing a
    /// read-only operation, and before and after processing operations in an atomic:operations request.
    /// </summary>
    WriteOperationKind? WriteOperation { get; }

    /// <summary>
    /// In case of an atomic:operations request, identifies the overarching transaction.
    /// </summary>
    string? TransactionId { get; }

    /// <summary>
    /// The JSON:API extensions enabled for the current request. This is always a subset of <see cref="IJsonApiOptions.Extensions" />.
    /// </summary>
    IReadOnlySet<JsonApiMediaTypeExtension> Extensions { get; }

    /// <summary>
    /// Performs a shallow copy.
    /// </summary>
    void CopyFrom(IJsonApiRequest other);
}
