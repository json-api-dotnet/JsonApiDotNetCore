using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Options to configure links at a global level.
    /// </summary>
    public interface ILinksConfiguration
    {
        /// <summary>
        /// Use relative links for all resources.
        /// </summary>
        /// <example>
        /// <code>
        /// options.RelativeLinks = true;
        /// </code>
        /// <code>
        /// {
        ///   "type": "articles",
        ///   "id": "4309",
        ///   "relationships": {
        ///      "author": {
        ///        "links": {
        ///          "self": "/api/v1/articles/4309/relationships/author",
        ///          "related": "/api/v1/articles/4309/author"
        ///        }
        ///      }
        ///   }
        /// }
        /// </code>
        /// </example>
        bool RelativeLinks { get; }
        /// <summary>
        /// Configures globally which links to show in the <see cref="TopLevelLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// adding a <see cref="LinksAttribute"/> to the class definition of that resource.
        /// </summary>
        Link TopLevelLinks { get; }

        /// <summary>
        /// Configures globally which links to show in the <see cref="ResourceLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// adding a <see cref="LinksAttribute"/> to the class definition of that resource.
        /// </summary>
        Link ResourceLinks { get; }
        /// <summary>
        /// Configures globally which links to show in the <see cref="RelationshipLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// adding a <see cref="LinksAttribute"/> to the class definition of that resource.
        /// Option can also be specified per relationship by using the associated links argument
        /// in the constructor of <see cref="RelationshipAttribute"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// options.DefaultRelationshipLinks = Link.None;
        /// </code>
        /// <code>
        /// {
        ///   "type": "articles",
        ///   "id": "4309",
        ///   "relationships": {
        ///      "author": { "data": { "type": "people", "id": "1234" }
        ///      }
        ///   }
        /// }
        /// </code>
        /// </example>
        Link RelationshipLinks { get; }

    }
}