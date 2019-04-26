using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{

    /// <summary>
    /// A singleton service for a particular TEntity that stores a field of 
    /// enums that represents which resource hooks have been implemented for that
    /// particular entity.
    /// </summary>
    public interface IHooksDiscovery<TEntity> : IHooksDiscovery where TEntity : class, IIdentifiable
    {

    }


    public interface IHooksDiscovery
    {
        /// <summary>
        /// A list of the implemented hooks for resource TEntity
        /// </summary>
        /// <value>The implemented hooks.</value>
        ResourceHook[] ImplementedHooks { get; }
    }

}
