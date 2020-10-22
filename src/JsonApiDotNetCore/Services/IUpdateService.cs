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
        /// Handles a json:api request to update an existing resource with attributes, relationships or both. May contain a partial set of attributes.
        /// </summary>
        Task<TResource> UpdateAsync(TId id, TResource resource);
    }
}
