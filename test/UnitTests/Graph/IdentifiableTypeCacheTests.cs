using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using UnitTests.Internal;
using Xunit;

namespace UnitTests.Graph
{
    public class IdentifiableTypeCacheTests
    {
        [Fact]
        public void GetIdentifiableTypes_Locates_Identifiable_Resource()
        {
            // Arrange
            var resourceType = typeof(Model);
            var typeCache = new IdentifiableTypeCache();

            // Act
            var results = typeCache.GetIdentifiableTypes(resourceType.Assembly);

            // Assert
            Assert.Contains(results, r => r.ResourceType == resourceType);
        }

        [Fact]
        public void GetIdentifiableTypes_Only_Contains_IIdentifiable_Types()
        {
            // Arrange
            var resourceType = typeof(Model);
            var typeCache = new IdentifiableTypeCache();

            // Act
            var resourceDescriptors = typeCache.GetIdentifiableTypes(resourceType.Assembly);

            // Assert
            foreach(var resourceDescriptor in resourceDescriptors)
                Assert.True(typeof(IIdentifiable).IsAssignableFrom(resourceDescriptor.ResourceType));
        }
    }
}
