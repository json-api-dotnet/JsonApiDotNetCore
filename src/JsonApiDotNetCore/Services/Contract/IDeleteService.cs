using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IDeleteService<T> : IDeleteService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IDeleteService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task DeleteAsync(TId id);
    }
}
