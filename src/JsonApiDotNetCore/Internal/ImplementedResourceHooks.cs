using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public enum ResourceAction
    {
        Get,
        GetSingle,
        GetRelationship,
        Create,
        Patch,
        PatchRelationships,
        Delete
    }

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
                throw new JsonApiSetupException($@" Implemented hooks may be discovered only once.
                    Adding such implementations at runtime is currently not supported.");
            }

            // Do reflective discovery of implemented hooks:
            // eg, for a model Article, it should  discover if there is declared a class
            // ResourceDefinition<Article>, and if so, will reflectively discover
            // which of the methods of IResourceHookContainer<Article> have a 
            // custom implementation. For these methods, include them in a 
            // ResourceHook[] and the publically ImplementedHooks.
            // Hardcoding this for now.
            ImplementedHooks = new ResourceHook[] {
                    ResourceHook.BeforeCreate,
                    ResourceHook.AfterCreate,
                    ResourceHook.BeforeRead,
                    ResourceHook.BeforeUpdate,
                    ResourceHook.AfterUpdate,
                    ResourceHook.BeforeDelete,
                    ResourceHook.AfterDelete
                };

        }
    }
}
