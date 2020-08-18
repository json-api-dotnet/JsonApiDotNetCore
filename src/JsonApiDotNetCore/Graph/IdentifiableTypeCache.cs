using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<Assembly, IReadOnlyCollection<ResourceDescriptor>> _typeCache = new ConcurrentDictionary<Assembly, IReadOnlyCollection<ResourceDescriptor>>();

        /// <summary>
        /// Get all implementations of <see cref="IIdentifiable"/> in the assembly
        /// </summary>
        public IReadOnlyCollection<ResourceDescriptor> GetIdentifiableTypes(Assembly assembly)
        {
            return _typeCache.GetOrAdd(assembly, asm => FindIdentifiableTypes(asm).ToArray());
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
