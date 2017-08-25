using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Builders
{
    public interface IContextGraphBuilder
    {
        Link DocumentLinks  { get; set; }
        IContextGraph Build();
        void AddResource<TResource>(string pluralizedTypeName) where TResource : class, IIdentifiable<int>;
        void AddResource<TResource, TId>(string pluralizedTypeName) where TResource : class, IIdentifiable<TId>;
        void AddDbContext<T>() where T : DbContext;
    }
}
