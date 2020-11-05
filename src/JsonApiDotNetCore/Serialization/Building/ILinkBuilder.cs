using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
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
        /// Builds the links object for resources in the primary data.
        /// </summary>
        ResourceLinks GetResourceLinks(string resourceName, string id);

        /// <summary>
        /// Builds the links object that is included in the values of the <see cref="RelationshipEntry"/>.
        /// </summary>
        RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent);
    }
}
