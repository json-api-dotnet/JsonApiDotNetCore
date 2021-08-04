using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IAddToRelationshipService<TResource> : IAddToRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary />
    [PublicAPI]
    public interface IAddToRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to add resources to a to-many relationship.
        /// </summary>
        /// <param name="leftId">
        /// Identifies the left side of the relationship.
        /// </param>
        /// <param name="relationshipName">
        /// The relationship to add resources to.
        /// </param>
        /// <param name="rightResourceIds">
        /// The set of resources to add to the relationship.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task AddToToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken);
    }
}
