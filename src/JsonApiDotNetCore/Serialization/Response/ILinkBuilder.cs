#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response
{
    /// <summary>
    /// Builds resource object links and relationship object links.
    /// </summary>
    public interface ILinkBuilder
    {
        /// <summary>
        /// Builds the links object that is included in the top-level of the document.
        /// </summary>
        TopLevelLinks GetTopLevelLinks();

        /// <summary>
        /// Builds the links object for a returned resource (primary or included).
        /// </summary>
        ResourceLinks GetResourceLinks(ResourceType resourceType, string id);

        /// <summary>
        /// Builds the links object for a relationship inside a returned resource.
        /// </summary>
        RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource);
    }
}
