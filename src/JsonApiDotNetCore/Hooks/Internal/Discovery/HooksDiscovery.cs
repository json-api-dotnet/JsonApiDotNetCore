using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    /// <summary>
    /// The default implementation for IHooksDiscovery
    /// </summary>
    public class HooksDiscovery<TResource> : IHooksDiscovery<TResource> where TResource : class, IIdentifiable
    {
        private readonly Type _boundResourceDefinitionType = typeof(ResourceDefinition<TResource>);
        private readonly ResourceHook[] _allHooks;
        private readonly ResourceHook[] _databaseValuesAttributeAllowed =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeUpdateRelationship,
            ResourceHook.BeforeDelete
        };

        /// <inheritdoc />
        public ResourceHook[] ImplementedHooks { get; private set; }
        public ResourceHook[] DatabaseValuesEnabledHooks { get; private set; }
        public ResourceHook[] DatabaseValuesDisabledHooks { get; private set; }

        public HooksDiscovery(IServiceProvider provider)
        {
            _allHooks = Enum.GetValues(typeof(ResourceHook))
                            .Cast<ResourceHook>()
                            .Where(h => h != ResourceHook.None)
                            .ToArray();

            Type containerType;
            using (var scope = provider.CreateScope())
            {
                containerType = scope.ServiceProvider.GetService(_boundResourceDefinitionType)?.GetType();
            }

            DiscoverImplementedHooks(containerType);
        }

        /// <summary>
        /// Discovers the implemented hooks for a model.
        /// </summary>
        /// <returns>The implemented hooks for model.</returns>
        private void DiscoverImplementedHooks(Type containerType)
        {
            if (containerType == null || containerType == _boundResourceDefinitionType)
            {
                return;
            }

            var implementedHooks = new List<ResourceHook>();
            // this hook can only be used with enabled database values
            var databaseValuesEnabledHooks = new List<ResourceHook> { ResourceHook.BeforeImplicitUpdateRelationship };
            var databaseValuesDisabledHooks = new List<ResourceHook>();
            foreach (var hook in _allHooks)
            {
                var method = containerType.GetMethod(hook.ToString("G"));
                if (method.DeclaringType == _boundResourceDefinitionType)
                    continue;

                implementedHooks.Add(hook);
                var attr = method.GetCustomAttributes(true).OfType<LoadDatabaseValuesAttribute>().SingleOrDefault();
                if (attr != null)
                {
                    if (!_databaseValuesAttributeAllowed.Contains(hook))
                    {
                        throw new InvalidConfigurationException($"{nameof(LoadDatabaseValuesAttribute)} cannot be used on hook" +
                            $"{hook:G} in resource definition  {containerType.Name}");
                    }
                    var targetList = attr.Value ? databaseValuesEnabledHooks : databaseValuesDisabledHooks;
                    targetList.Add(hook);
                }
            }

            ImplementedHooks = implementedHooks.ToArray();
            DatabaseValuesDisabledHooks = databaseValuesDisabledHooks.ToArray();
            DatabaseValuesEnabledHooks = databaseValuesEnabledHooks.ToArray();
        }
    }
}
