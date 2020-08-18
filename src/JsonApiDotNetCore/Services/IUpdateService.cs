using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdateService<TResource> : IUpdateService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IUpdateService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<TResource> UpdateAsync(TId id, TResource resource);
    }
}
