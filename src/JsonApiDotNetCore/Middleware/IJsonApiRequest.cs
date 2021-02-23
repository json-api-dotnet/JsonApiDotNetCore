using System;
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
        /// The request URL prefix. This may be an absolute or relative path, depending on <see cref="IJsonApiOptions.UseRelativeLinks" />.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// Absolute: https://example.com/api/v1
        /// Relative: /api/v1
        /// ]]></code>
        /// </example>
        string BasePath { get; }

        /// <summary>
        /// The ID of the primary (top-level) resource for this request. This would be null in "/blogs", "123" in "/blogs/123" or "/blogs/123/author".
        /// </summary>
        string PrimaryId { get; }

        /// <summary>
        /// The primary (top-level) resource for this request. This would be "blogs" in "/blogs", "/blogs/123" or "/blogs/123/author".
        /// </summary>
        ResourceContext PrimaryResource { get; }

        /// <summary>
        /// The secondary (nested) resource for this request. This would be null in "/blogs", "/blogs/123" and "/blogs/123/unknownResource" or "people" in
        /// "/blogs/123/author" and "/blogs/123/relationships/author".
        /// </summary>
        ResourceContext SecondaryResource { get; }

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
        /// In case of an atomic:operations request, this indicates the kind of operation currently being processed.
        /// </summary>
        OperationKind? OperationKind { get; }

        /// <summary>
        /// In case of an atomic:operations request, identifies the overarching transaction.
        /// </summary>
        Guid? TransactionId { get; }

        /// <summary>
        /// Performs a shallow copy.
        /// </summary>
        void CopyFrom(IJsonApiRequest other);
    }
}
