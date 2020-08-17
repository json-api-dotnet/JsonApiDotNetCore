using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface ICreateService<T> : ICreateService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface ICreateService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<T> CreateAsync(T resource);
    }
}
