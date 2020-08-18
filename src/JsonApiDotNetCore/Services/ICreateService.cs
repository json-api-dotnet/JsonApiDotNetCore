using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface ICreateService<TResource> : ICreateService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface ICreateService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<TResource> CreateAsync(TResource resource);
    }
}
