using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetSecondaryService<T> : IGetSecondaryService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetSecondaryService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<object> GetSecondaryAsync(TId id, string relationshipName);
    }
}
