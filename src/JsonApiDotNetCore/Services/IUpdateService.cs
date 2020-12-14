using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IUpdateService<TResource> : IUpdateService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface IUpdateService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to update the attributes and/or relationships of an existing resource.
        /// Only the values of sent attributes are replaced. And only the values of sent relationships are replaced.
        /// </summary>
        Task<TResource> UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken);
    }
}
