using System.Linq.Expressions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdateValueHelper<TResource> where TResource : IIdentifiable
    {
        void MarkUpdated(Expression<System.Func<TResource, dynamic>> selector);
    }
}