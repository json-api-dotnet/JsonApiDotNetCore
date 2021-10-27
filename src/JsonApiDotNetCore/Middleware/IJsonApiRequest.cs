using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Middleware
{
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
        /// The ID of the primary (top-level) resource for this request. This would be null in "/blogs", "123" in "/blogs/123" or "/blogs/123/author".
        /// </summary>
        string PrimaryId { get; }

        /// <summary>
        /// The primary (top-level) resource type for this request. This would be "blogs" in "/blogs", "/blogs/123" or "/blogs/123/author".
        /// </summary>
        ResourceType PrimaryResourceType { get; }

        /// <summary>
        /// The secondary (nested) resource type for this request. This would be null in "/blogs", "/blogs/123" and "/blogs/123/unknownResource" or "people" in
        /// "/blogs/123/author" and "/blogs/123/relationships/author".
        /// </summary>
        ResourceType SecondaryResourceType { get; }

        /// <summary>
        /// The relationship for this nested request. This would be null in "/blogs", "/blogs/123" and "/blogs/123/unknownResource" or "author" in
        /// "/blogs/123/author" and "/blogs/123/relationships/author".
        /// </summary>
        RelationshipAttribute Relationship { get; }

        /// <summary>
        /// Indicates whether this request targets a single resource or a collection of resources.
        /// </summary>
        bool IsCollection { get; }

        /// <summary>
        /// Indicates whether this request targets only fetching of data (such as resources and relationships).
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// In case of a non-readonly request, this indicates the kind of write operation currently being processed.
        /// </summary>
        WriteOperationKind? WriteOperation { get; }

        /// <summary>
        /// In case of an atomic:operations request, identifies the overarching transaction.
        /// </summary>
        string TransactionId { get; }

        /// <summary>
        /// Performs a shallow copy.
        /// </summary>
        void CopyFrom(IJsonApiRequest other);
    }
}
