using System;
using System.Linq.Expressions;

namespace JsonApiDotNetCore.Resources
{
    public interface IResourceFactory
    {
        public object CreateInstance(Type resourceType);
        public TResource CreateInstance<TResource>();
        public NewExpression CreateNewExpression(Type resourceType);
    }
}
