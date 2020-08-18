using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IDeleteService<TResource> : IDeleteService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IDeleteService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task DeleteAsync(TId id);
    }
}
