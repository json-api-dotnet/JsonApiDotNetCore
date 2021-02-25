using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Graph
{
    public sealed class ResourceDescriptorAssemblyCacheTests
    {
        [Fact]
        public void GetResourceDescriptorsPerAssembly_Locates_Identifiable_Resource()
        {
            // Arrange
            Type resourceType = typeof(Model);

            var assemblyCache = new ResourceDescriptorAssemblyCache();
            assemblyCache.RegisterAssembly(resourceType.Assembly);

            // Act
            IEnumerable<(Assembly assembly, IReadOnlyCollection<ResourceDescriptor> resourceDescriptors)> results =
                assemblyCache.GetResourceDescriptorsPerAssembly();

            // Assert
            Assert.Contains(results,
                result => result.resourceDescriptors != null && result.resourceDescriptors.Any(descriptor => descriptor.ResourceType == resourceType));
        }

        [Fact]
        public void GetResourceDescriptorsPerAssembly_Only_Contains_IIdentifiable_Types()
        {
            // Arrange
            Type resourceType = typeof(Model);

            var assemblyCache = new ResourceDescriptorAssemblyCache();
            assemblyCache.RegisterAssembly(resourceType.Assembly);

            // Act
            IEnumerable<(Assembly assembly, IReadOnlyCollection<ResourceDescriptor> resourceDescriptors)> results =
                assemblyCache.GetResourceDescriptorsPerAssembly();

            // Assert
            foreach (ResourceDescriptor resourceDescriptor in results.SelectMany(result => result.resourceDescriptors))
            {
                Assert.True(typeof(IIdentifiable).IsAssignableFrom(resourceDescriptor.ResourceType));
            }
        }
    }
}
