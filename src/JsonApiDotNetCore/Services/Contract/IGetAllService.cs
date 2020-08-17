using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetAllService<T> : IGetAllService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetAllService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<IReadOnlyCollection<T>> GetAsync();
    }
}
