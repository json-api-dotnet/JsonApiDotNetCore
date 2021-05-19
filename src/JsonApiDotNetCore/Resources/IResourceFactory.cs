using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Repositories;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Creates object instances for resource classes, which may have injectable dependencies.
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Creates a new resource object instance.
        /// </summary>
        public IIdentifiable CreateInstance(Type resourceType);

        /// <summary>
        /// Creates a new resource object instance.
        /// </summary>
        public TResource CreateInstance<TResource>()
            where TResource : IIdentifiable;

        /// <summary>
        /// Returns an expression tree that represents creating a new resource object instance.
        /// </summary>
        public NewExpression CreateNewExpression(Type resourceType);

        /// <summary>
        /// Provides access to the request-scoped <see cref="IResourceDefinitionAccessor" /> instance. This method has been added solely to prevent introducing a
        /// breaking change in the <see cref="EntityFrameworkCoreRepository{TResource,TId}" /> constructor and will be removed in the next major version.
        /// </summary>
        [Obsolete]
        IResourceDefinitionAccessor GetResourceDefinitionAccessor();
    }
}
