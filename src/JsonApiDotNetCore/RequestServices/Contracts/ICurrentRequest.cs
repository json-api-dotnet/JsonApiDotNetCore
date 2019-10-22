using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Managers.Contracts
{
    /// <summary>
    /// Metadata associated to the current json:api request.
    /// </summary>
    public interface ICurrentRequest
    {
        /// <summary>
        /// The request namespace. This may be an absolute or relative path
        /// depending upon the configuration.
        /// </summary>
        /// <example>
        /// Absolute: https://example.com/api/v1
        /// 
        /// Relative: /api/v1
        /// </example>
        string BasePath { get; set; }

        /// <summary>
        /// If the request is on the `{id}/relationships/{relationshipName}` route
        /// </summary>
        bool IsRelationshipPath { get; set; }

        /// <summary>
        /// If <see cref="IsRelationshipPath"/> is true, this property
        /// is the relationship attribute associated with the targeted relationship
        /// </summary>
        RelationshipAttribute RequestRelationship { get; set; }

        /// <summary>
        /// Sets the current context entity for this entire request
        /// </summary>
        /// <param name="currentResourceContext"></param>
        void SetRequestResource(ResourceContext currentResourceContext);

        ResourceContext GetRequestResource();
    }
}
