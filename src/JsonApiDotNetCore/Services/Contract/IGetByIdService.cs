using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetByIdService<T> : IGetByIdService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetByIdService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<T> GetAsync(TId id);
    }
}
