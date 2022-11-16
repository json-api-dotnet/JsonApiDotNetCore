using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IGetAllService<T> : IGetAllService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetAllService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<IEnumerable<T>> GetAsync();
    }
}
