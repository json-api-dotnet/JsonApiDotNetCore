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
        None, // https://stackoverflow.com/questions/24151354/is-it-a-good-practice-to-add-a-null-or-none-member-to-the-enum
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
    /// enums that represents which resource hooks have been implemented for that
    /// particular entity.
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
        private readonly ResourceHook[] _allHooks;
        private bool _isInitialized;
        public ResourceHook[] ImplementedHooks { get; private set; }

        public ImplementedResourceHooks()
        {
            _allHooks = Enum.GetValues(typeof(ResourceHook))
                            .Cast<ResourceHook>()
                            .Where(h => h != ResourceHook.None)
                            .ToArray();
            DiscoverImplementedHooksForModel();
        }

        /// <summary>
        /// Discovers the implemented hooks for a model.
        /// </summary>
        /// <returns>The implemented hooks for model.</returns>
        void DiscoverImplementedHooksForModel()
        {
            var derivedTypes = TypeLocator.GetDerivedTypes(typeof(TEntity).Assembly, typeof(IResourceHookContainer<TEntity>));
            try
            {
                Type targetType = derivedTypes.SingleOrDefault(); // multiple containers is not supported
                if (targetType != null)
                {
                    ImplementedHooks = _allHooks.Where(h => targetType.GetMethod(h.ToString("G")).DeclaringType == targetType)
                                                .ToArray();
                }
                else
                {
                    ImplementedHooks = new ResourceHook[0];
                }
            } catch (Exception e)
            {
                throw new JsonApiSetupException($@"Incorrect resource hook setup. For a given model of type TEntity, 
                only one class may implement IResourceHookContainer<TEntity>");
            }

        }
    }
}
