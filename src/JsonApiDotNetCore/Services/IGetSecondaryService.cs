using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IGetSecondaryService<TResource> : IGetSecondaryService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IGetSecondaryService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<object> GetSecondaryAsync(TId id, string relationshipName);
    }
}
