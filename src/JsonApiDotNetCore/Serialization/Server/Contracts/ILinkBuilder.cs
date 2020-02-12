using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Serialization.Server.Builders
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
        /// <param name="relationship"></param>
        /// <param name="parent"></param>
        RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent);
    }
}
