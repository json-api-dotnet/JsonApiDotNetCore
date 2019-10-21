using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Contracts
{
    /// <summary>
    /// A cache for the models in entity core
    /// TODO: separate context entity getting part from relationship resolving part.
    /// These are two deviating responsibilities that often do not need to be exposed
    /// at the same time.
    /// </summary>
    public interface IResourceGraph : IContextEntityProvider
    {
        RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship);

        /// <summary>
        /// Get the internal navigation property name for the specified public
        /// relationship name.
        /// </summary>
        /// <param name="relationshipName">The public relationship name specified by a <see cref="HasOneAttribute" /> or <see cref="HasManyAttribute" /></param>
        /// <example>
        /// <code>
        /// _graph.GetRelationshipName&lt;TodoItem&gt;("achieved-date");
        /// // returns "AchievedDate"
        /// </code>
        /// </example>
        string GetRelationshipName<TParent>(string relationshipName);

        /// <summary>
        /// Get the public attribute name for a type based on the internal attribute name.
        /// </summary>
        /// <param name="internalAttributeName">The internal attribute name for a <see cref="AttrAttribute" />.</param>
        string GetPublicAttributeName<TParent>(string internalAttributeName);

        /// <summary>
        /// Was built against an EntityFrameworkCore DbContext ?
        /// </summary>
        bool UsesDbContext { get; }

        ContextEntity GetEntityFromControllerName(string pathParsed);
    }
}
