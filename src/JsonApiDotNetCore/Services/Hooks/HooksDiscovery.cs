using System;
using System.Linq;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// The default implementation for IHooksDiscovery
    /// </summary>
    public class HooksDiscovery<TEntity> : IHooksDiscovery<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly ResourceHook[] _allHooks;

        /// <inheritdoc/>
        public ResourceHook[] ImplementedHooks { get; private set; }

        public HooksDiscovery()
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
            var derivedTypes = TypeLocator.GetDerivedTypes(typeof(TEntity).Assembly, typeof(ResourceDefinition<TEntity>)).ToList();
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
