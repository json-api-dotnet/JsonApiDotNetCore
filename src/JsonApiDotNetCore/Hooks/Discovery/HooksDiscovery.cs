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


            var implementedHooks = new List<ResourceHook>();
            var enabledHooks = new List<ResourceHook>() { ResourceHook.BeforeImplicitUpdateRelationship } ;
            var disabledHooks = new List<ResourceHook>();
            Type targetType = null;
            try
            {
                targetType = derivedTypes.SingleOrDefault(); // multiple containers is not supported
            }
            catch
            {
                throw new JsonApiSetupException($"It is currently not supported to" +
                	"implement hooks across multiple implementations of ResourceDefinition<T>");
            }
            if (targetType != null)
            {
                foreach (var hook in _allHooks)
                {
                    var method = targetType.GetMethod(hook.ToString("G"));
                    if (method.DeclaringType != parameterizedResourceDefinition)
                    {
                        implementedHooks.Add(hook);
                        var attr = method.GetCustomAttributes(true).OfType<LoadDatabaseValues>().SingleOrDefault();
                        if (attr != null)
                        {
                            if (!_databaseValuesAttributeAllowed.Contains(hook))
                            {
                                throw new JsonApiSetupException($"DatabaseValuesAttribute cannot be used on hook" +
                                    $"{hook.ToString("G")} in resource definition  {parameterizedResourceDefinition.Name}");
                            }
                            var targetList = attr.value ? enabledHooks : disabledHooks;
                            targetList.Add(hook);
                        }
                     }
                }

            }
            ImplementedHooks = implementedHooks.ToArray();
            DatabaseValuesDisabledHooks = disabledHooks.ToArray();
            DatabaseValuesEnabledHooks = enabledHooks.ToArray();

        }
    }
}
