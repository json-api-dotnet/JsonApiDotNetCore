using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface ISetRelationshipService<TResource> : ISetRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface ISetRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to perform a complete replacement of a relationship on an existing resource.
        /// </summary>
        /// <param name="primaryId">The identifier of the primary resource.</param>
        /// <param name="relationshipName">The relationship for which to perform a complete replacement.</param>
        /// <param name="secondaryResourceIds">The resource or set of resources to assign to the relationship.</param>
        /// <param name="cancellationToken">Propagates notification that request handling should be canceled.</param>
        Task SetRelationshipAsync(TId primaryId, string relationshipName, object secondaryResourceIds, CancellationToken cancellationToken);
    }
}
