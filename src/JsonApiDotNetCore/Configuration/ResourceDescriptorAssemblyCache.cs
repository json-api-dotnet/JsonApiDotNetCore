using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to scan assemblies for types and cache them, to facilitate resource auto-discovery.
    /// </summary>
    internal sealed class ResourceDescriptorAssemblyCache
    {
        private readonly TypeLocator _typeLocator = new TypeLocator();

        private readonly Dictionary<Assembly, IReadOnlyCollection<ResourceDescriptor>> _resourceDescriptorsPerAssembly =
            new Dictionary<Assembly, IReadOnlyCollection<ResourceDescriptor>>();

        public void RegisterAssembly(Assembly assembly)
        {
            if (!_resourceDescriptorsPerAssembly.ContainsKey(assembly))
            {
                _resourceDescriptorsPerAssembly[assembly] = null;
            }
        }

        public IEnumerable<(Assembly assembly, IReadOnlyCollection<ResourceDescriptor> resourceDescriptors)> GetResourceDescriptorsPerAssembly()
        {
            EnsureAssembliesScanned();

            return _resourceDescriptorsPerAssembly.Select(pair => (pair.Key, pair.Value)).ToArray();
        }

        private void EnsureAssembliesScanned()
        {
            foreach (Assembly assemblyToScan in _resourceDescriptorsPerAssembly.Where(pair => pair.Value == null).Select(pair => pair.Key).ToArray())
            {
                _resourceDescriptorsPerAssembly[assemblyToScan] = ScanForResourceDescriptors(assemblyToScan).ToArray();
            }
        }

        private IEnumerable<ResourceDescriptor> ScanForResourceDescriptors(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                ResourceDescriptor resourceDescriptor = _typeLocator.TryGetResourceDescriptor(type);

                if (resourceDescriptor != null)
                {
                    yield return resourceDescriptor;
                }
            }
        }
    }
}
