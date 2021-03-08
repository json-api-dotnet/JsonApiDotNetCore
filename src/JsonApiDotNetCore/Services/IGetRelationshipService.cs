using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IGetRelationshipService<TResource> : IGetRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary />
    public interface IGetRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to retrieve a single relationship.
        /// </summary>
        Task<object> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken);
    }
}
