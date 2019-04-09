using System;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal
{
    public enum ResourceHook
    {
        BeforeCreate,
        AfterCreate,
        BeforeRead,
        AfterRead,
        BeforeUpdate,
        AfterUpdate,
        BeforeDelete,
        AfterDelete
    }

    /// <summary>
    /// A singleton service for a particular TEntity that stores a field of 
    /// enums that represents which hooks have been implemented for a particular
    /// entity.
    /// </summary>
    public interface IImplementedResourceHooks<TEntity> where TEntity : class, IIdentifiable
    {
        ResourceHook[] ImplementedHooks { get; }
    }

    /// <summary>
    /// The default implementation for IImplementedResourceHooks
    /// </summary>
    public class ImplementedResourceHooks<TEntity> : IImplementedResourceHooks<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly ResourceHook[] _allHooks = Enum.GetValues(typeof(ResourceHook)) as ResourceHook[];
        private bool _isInitialized;
        public ResourceHook[] ImplementedHooks { get; private set; }

        public ImplementedResourceHooks()
        {
            DiscoverImplementedHooksForModel();
        }

        /// <summary>
        /// Discovers the implemented hooks for a model.
        /// </summary>
        /// <returns>The implemented hooks for model.</returns>
        void DiscoverImplementedHooksForModel()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
            }
            else
            {
                throw new JsonApiSetupException($@"Implemented hooks may be discovered only once.
                    Adding such implementations at runtime is currently not supported.");
            }

           Type resourceDefinitionImplementationType = null;

            foreach (var match in TypeLocator.GetDerivedTypes(typeof(TEntity).Assembly, typeof(ResourceDefinition<TEntity>)))
            {
                resourceDefinitionImplementationType = match;
                break;
            }
            if (resourceDefinitionImplementationType != null)
            {
                ImplementedHooks = _allHooks.Where(h => resourceDefinitionImplementationType.GetMethod(h.ToString("G")).DeclaringType == resourceDefinitionImplementationType)
                                            .ToArray();
            } else
            {
                ImplementedHooks = new ResourceHook[0];
            }

        }
    }
}
