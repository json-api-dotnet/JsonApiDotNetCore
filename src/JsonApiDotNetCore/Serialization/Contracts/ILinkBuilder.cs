using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Builders
{
    public interface ILinkBuilder
    {
        /// <summary>
        /// Builds the links object that is included in the top-level of the document.
        /// </summary>
        TopLevelLinks GetTopLevelLinks();
        /// <summary>
        /// Builds the links object for resources in the primary data.
        /// </summary>
        /// <param name="id"></param>
        ResourceLinks GetResourceLinks(string resourceName, string id);
        /// <summary>
        /// Builds the links object that is included in the values of the <see cref="RelationshipData"/>.
        /// </summary>
        /// <param name="relationship"></param>
        /// <param name="parent"></param>
        RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent);
    }
}
