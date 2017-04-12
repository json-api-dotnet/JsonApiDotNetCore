using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Builders
{
    public interface IContextGraphBuilder
    {
        IContextGraph Build();
        void AddResource<TResource>(string pluralizedTypeName) where TResource : class;
    }
}
