using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Graph
{
    /// <summary>
    /// Used to cache and locate types, to facilitate auto-resource discovery
    /// </summary>
    internal sealed class IdentifiableTypeCache
    {
        private readonly IDictionary<Assembly, List<ResourceDescriptor>> _typeCache = new Dictionary<Assembly, List<ResourceDescriptor>>();

        /// <summary>
        /// Get all implementations of <see cref="IIdentifiable"/> in the assembly
        /// </summary>
        public IEnumerable<ResourceDescriptor> GetIdentifiableTypes(Assembly assembly)
        {
            if (!_typeCache.ContainsKey(assembly))
            {
                _typeCache[assembly] = FindIdentifiableTypes(assembly).ToList();
            }

            return _typeCache[assembly];
        }

        private static IEnumerable<ResourceDescriptor> FindIdentifiableTypes(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (TypeLocator.TryGetResourceDescriptor(type, out var descriptor))
                {
                    yield return descriptor;
                }
            }
        }
    }
}
