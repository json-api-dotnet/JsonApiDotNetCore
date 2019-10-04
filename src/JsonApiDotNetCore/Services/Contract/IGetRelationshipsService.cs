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
        Task<T> GetRelationshipsAsync(TId id, string relationshipName);
    }
}
