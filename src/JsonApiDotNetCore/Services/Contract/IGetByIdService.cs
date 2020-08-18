using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetByIdService<TResource> : IGetByIdService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IGetByIdService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<TResource> GetAsync(TId id);
    }
}
