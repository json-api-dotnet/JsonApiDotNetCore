using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdateRelationshipService<T> : IUpdateRelationshipService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IUpdateRelationshipService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships);
    }
}
