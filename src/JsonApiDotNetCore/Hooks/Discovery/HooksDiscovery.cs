using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// The default implementation for IHooksDiscovery
    /// </summary>
    public class HooksDiscovery<TEntity> : IHooksDiscovery<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly ResourceHook[] _allHooks;

        /// <inheritdoc/>
        public ResourceHook[] ImplementedHooks { get; private set; }
        public ResourceHook[] DatabaseDiffEnabledHooks { get; private set; }
        public ResourceHook[] DatabaseDiffDisabledHooks { get; private set; }


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
            Type parameterizedResourceDefinition = typeof(ResourceDefinition<TEntity>);
            var derivedTypes = TypeLocator.GetDerivedTypes(typeof(TEntity).Assembly, parameterizedResourceDefinition).ToList();
            try
            {
                var implementedHooks = new List<ResourceHook>();
                var diffEnabledHooks = new List<ResourceHook>();
                var diffDisabledHooks = new List<ResourceHook>();
                Type targetType = derivedTypes.SingleOrDefault(); // multiple containers is not supported
                if (targetType != null)
                {
                    foreach (var hook in _allHooks)
                    {
                        var method = targetType.GetMethod(hook.ToString("G"));
                        if (method.DeclaringType != parameterizedResourceDefinition)
                        {
                            implementedHooks.Add(hook);
                            if (hook == ResourceHook.BeforeImplicitUpdateRelationship)
                            {
                                diffEnabledHooks.Add(hook);
                                continue;
                            }
                            var attr = method.GetCustomAttributes(true).OfType<LoadDatabaseValues>().SingleOrDefault();
                            if (attr != null)
                            {
                                var targetList = attr.value ? diffEnabledHooks : diffDisabledHooks;
                                targetList.Add(hook);
                            }
                         }
                    }

                }
                ImplementedHooks = implementedHooks.ToArray();
                DatabaseDiffDisabledHooks = diffDisabledHooks.ToArray();
                DatabaseDiffEnabledHooks = diffEnabledHooks.ToArray();
            } catch (Exception e)
            {
                throw new JsonApiSetupException($@"Incorrect resource hook setup. For a given model of type TEntity, 
                only one class may implement IResourceHookContainer<TEntity>");
            }

        }
    }
}
