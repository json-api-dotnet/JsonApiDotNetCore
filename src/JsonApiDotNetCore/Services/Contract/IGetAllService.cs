using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetAllService<TResource> : IGetAllService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IGetAllService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<IReadOnlyCollection<TResource>> GetAsync();
    }
}
