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
        /// Was built against an EntityFrameworkCore DbContext ?
        /// </summary>
        bool UsesDbContext { get; }

        ContextEntity GetEntityFromControllerName(string pathParsed);
    }
}
