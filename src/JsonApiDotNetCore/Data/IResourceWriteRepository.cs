using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IResourceWriteRepository<TResource>
        : IResourceWriteRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceWriteRepository<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<TResource> CreateAsync(TResource entity);

        Task<TResource> UpdateAsync(TResource entity);

        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);

        Task<bool> DeleteAsync(TId id);
    }
}
