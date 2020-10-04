using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// A  helper dedicated to processing updates of relationships
    /// </summary>
    /// <remarks>
    /// This service is required to be able translate involved expressions into queries
    /// instead of having them evaluated on the client side. In particular, for all three types of relationship
    /// a lookup is performed based on an ID. Expressions that use IIdentifiable.StringId can never
    /// be translated into queries because this property only exists at runtime after the query is performed.
    /// We will have to build expression trees if we want to use IIdentifiable{TId}.TId, for which we minimally a
    /// generic execution to DbContext.Set{T}().
    /// </remarks>
    public interface IRepositoryRelationshipUpdateHelper
    {
        /// <summary>
        /// Processes updates of relationships
        /// </summary>
        Task UpdateRelationshipAsync(IIdentifiable parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds);
    }
}
