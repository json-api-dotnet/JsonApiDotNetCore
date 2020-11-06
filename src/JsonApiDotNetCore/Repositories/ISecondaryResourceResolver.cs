using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Provides methods to retrieve unassigned related resources using its matching repository.
    /// </summary>
    public interface ISecondaryResourceResolver
    {
        Task<ICollection<MissingResourceInRelationship>> GetMissingResourcesToAssignInRelationships(IIdentifiable leftResource);
        Task<ICollection<MissingResourceInRelationship>> GetMissingSecondaryResources(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds);
    }
}
