using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Graph
{
    public class ResourceDescriptorAssemblyCacheTests
    {
        [Fact]
        public void GetResourceDescriptorsPerAssembly_Locates_Identifiable_Resource()
        {
            // Arrange
            var resourceType = typeof(Model);
            
            var assemblyCache = new ResourceDescriptorAssemblyCache();
            assemblyCache.RegisterAssembly(resourceType.Assembly);

            // Act
            var results = assemblyCache.GetResourceDescriptorsPerAssembly();

            // Assert
            Assert.Contains(results, result => result.resourceDescriptors != null && 
                result.resourceDescriptors.Any(descriptor => descriptor.ResourceType == resourceType));
        }

        [Fact]
        public void GetResourceDescriptorsPerAssembly_Only_Contains_IIdentifiable_Types()
        {
            // Arrange
            var resourceType = typeof(Model);

            var assemblyCache = new ResourceDescriptorAssemblyCache();
            assemblyCache.RegisterAssembly(resourceType.Assembly);

            // Act
            var results = assemblyCache.GetResourceDescriptorsPerAssembly();

            // Assert
            foreach (var resourceDescriptor in results.SelectMany(result => result.resourceDescriptors))
            {
                Assert.True(typeof(IIdentifiable).IsAssignableFrom(resourceDescriptor.ResourceType));
            }
        }
    }
}
