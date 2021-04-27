using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Resources;

namespace UnitTests
{
    internal sealed class TestResourceFactory : IResourceFactory
    {
        public IIdentifiable CreateInstance(Type resourceType)
        {
            return (IIdentifiable)Activator.CreateInstance(resourceType);
        }

        public TResource CreateInstance<TResource>()
            where TResource : IIdentifiable
        {
            return (TResource)Activator.CreateInstance(typeof(TResource));
        }

        public NewExpression CreateNewExpression(Type resourceType)
        {
            return Expression.New(resourceType);
        }

        public IResourceDefinitionAccessor GetResourceDefinitionAccessor()
        {
            return new NeverResourceDefinitionAccessor();
        }
    }
}
