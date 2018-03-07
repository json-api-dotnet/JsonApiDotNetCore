using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdateService<T> : IUpdateService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IUpdateService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<T> UpdateAsync(TId id, T entity);
    }
}
