using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetRelationshipsService<T> : IGetRelationshipsService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetRelationshipsService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<object> GetRelationshipsAsync(TId id, string relationshipName);
    }
}
