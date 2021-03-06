using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    /// <summary>
    /// The default implementation for IHooksDiscovery
    /// </summary>
    [PublicAPI]
    public class HooksDiscovery<TResource> : IHooksDiscovery<TResource>
        where TResource : class, IIdentifiable
    {
        private readonly Type _boundResourceDefinitionType = typeof(ResourceHooksDefinition<TResource>);
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
            _allHooks = Enum.GetValues(typeof(ResourceHook)).Cast<ResourceHook>().Where(hook => hook != ResourceHook.None).ToArray();

            Type containerType;

            using (IServiceScope scope = provider.CreateScope())
            {
                containerType = scope.ServiceProvider.GetService(_boundResourceDefinitionType)?.GetType();
            }

            DiscoverImplementedHooks(containerType);
        }

        /// <summary>
        /// Discovers the implemented hooks for a model.
        /// </summary>
        /// <returns>
        /// The implemented hooks for model.
        /// </returns>
        private void DiscoverImplementedHooks(Type containerType)
        {
            if (containerType == null || containerType == _boundResourceDefinitionType)
            {
                return;
            }

            var implementedHooks = new List<ResourceHook>();
            // this hook can only be used with enabled database values
            List<ResourceHook> databaseValuesEnabledHooks = ResourceHook.BeforeImplicitUpdateRelationship.AsList();
            var databaseValuesDisabledHooks = new List<ResourceHook>();

            foreach (ResourceHook hook in _allHooks)
            {
                MethodInfo method = containerType.GetMethod(hook.ToString("G"));

                if (method == null || method.DeclaringType == _boundResourceDefinitionType)
                {
                    continue;
                }

                implementedHooks.Add(hook);
                LoadDatabaseValuesAttribute attr = method.GetCustomAttributes(true).OfType<LoadDatabaseValuesAttribute>().SingleOrDefault();

                if (attr != null)
                {
                    if (!_databaseValuesAttributeAllowed.Contains(hook))
                    {
                        throw new InvalidConfigurationException($"{nameof(LoadDatabaseValuesAttribute)} cannot be used on hook" +
                            $"{hook:G} in resource definition  {containerType.Name}");
                    }

                    List<ResourceHook> targetList = attr.Value ? databaseValuesEnabledHooks : databaseValuesDisabledHooks;
                    targetList.Add(hook);
                }
            }

            ImplementedHooks = implementedHooks.ToArray();
            DatabaseValuesDisabledHooks = databaseValuesDisabledHooks.ToArray();
            DatabaseValuesEnabledHooks = databaseValuesEnabledHooks.ToArray();
        }
    }
}
