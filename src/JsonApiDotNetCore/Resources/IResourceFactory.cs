using System;
using System.Linq.Expressions;

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
        public object CreateInstance(Type resourceType);
        
        /// <summary>
        /// Creates a new resource object instance.
        /// </summary>
        public TResource CreateInstance<TResource>(object id = null) where TResource : IIdentifiable;

        /// <summary>
        /// Returns an expression tree that represents creating a new resource object instance.
        /// </summary>
        public NewExpression CreateNewExpression(Type resourceType);
    }
}
