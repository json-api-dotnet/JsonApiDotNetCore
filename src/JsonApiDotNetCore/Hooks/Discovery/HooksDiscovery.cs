using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// The default implementation for IHooksDiscovery
    /// </summary>
    public class HooksDiscovery<TEntity> : IHooksDiscovery<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly Type _boundResourceDefinitionType = typeof(ResourceDefinition<TEntity>);
        private readonly ResourceHook[] _allHooks;
        private readonly ResourceHook[] _databaseValuesAttributeAllowed =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeUpdateRelationship,
            ResourceHook.BeforeDelete
        };

        /// <inheritdoc/>
        public ResourceHook[] ImplementedHooks { get; private set; }
        public ResourceHook[] DatabaseValuesEnabledHooks { get; private set; }
        public ResourceHook[] DatabaseValuesDisabledHooks { get; private set; }

        public HooksDiscovery(IScopedServiceProvider provider)
        {
            _allHooks = Enum.GetValues(typeof(ResourceHook))
                            .Cast<ResourceHook>()
                            .Where(h => h != ResourceHook.None)
                            .ToArray();
            var containerType = provider.GetService(_boundResourceDefinitionType)?.GetType();
            if (containerType == null || containerType == _boundResourceDefinitionType)
                return;
            DiscoverImplementedHooksForModel(containerType);
        }

        /// <summary>
        /// Discovers the implemented hooks for a model.
        /// </summary>
        /// <returns>The implemented hooks for model.</returns>
        void DiscoverImplementedHooksForModel(Type containerType)
        {
            var implementedHooks = new List<ResourceHook>();
            var enabledHooks = new List<ResourceHook> { ResourceHook.BeforeImplicitUpdateRelationship };
            var disabledHooks = new List<ResourceHook>();
            foreach (var hook in _allHooks)
            {
                var method = containerType.GetMethod(hook.ToString("G"));
                if (method.DeclaringType == _boundResourceDefinitionType)
                    continue;

                implementedHooks.Add(hook);
                var attr = method.GetCustomAttributes(true).OfType<LoadDatabaseValues>().SingleOrDefault();
                if (attr != null)
                {
                    if (!_databaseValuesAttributeAllowed.Contains(hook))
                    {
                        throw new JsonApiSetupException($"DatabaseValuesAttribute cannot be used on hook" +
                            $"{hook.ToString("G")} in resource definition  {containerType.Name}");
                    }
                    var targetList = attr.value ? enabledHooks : disabledHooks;
                    targetList.Add(hook);
                }
            }

            ImplementedHooks = implementedHooks.ToArray();
            DatabaseValuesDisabledHooks = disabledHooks.ToArray();
            DatabaseValuesEnabledHooks = enabledHooks.ToArray();
        }
    }
}