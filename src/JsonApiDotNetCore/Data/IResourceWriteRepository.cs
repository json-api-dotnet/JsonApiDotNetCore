using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Data
{
    public interface IResourceWriteRepository<in TResource>
        : IResourceWriteRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceWriteRepository<in TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task CreateAsync(TResource resource);

        Task UpdateAsync(TResource requestResource, TResource databaseResource);

        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds);

        Task<bool> DeleteAsync(TId id);

        void FlushFromCache(TResource resource);
    }
}
