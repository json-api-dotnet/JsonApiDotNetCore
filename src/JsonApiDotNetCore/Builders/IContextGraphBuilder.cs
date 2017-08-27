using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Builders
{
    public interface IContextGraphBuilder
    {
        Link DocumentLinks  { get; set; }
        IContextGraph Build();
        IContextGraphBuilder AddResource<TResource>(string pluralizedTypeName) where TResource : class, IIdentifiable<int>;
        IContextGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName) where TResource : class, IIdentifiable<TId>;
        IContextGraphBuilder AddDbContext<T>() where T : DbContext;
    }
}
