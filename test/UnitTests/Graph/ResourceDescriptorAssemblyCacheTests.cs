using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using TestBuildingBlocks;
using Xunit;

namespace UnitTests.Graph;

public sealed class ResourceDescriptorAssemblyCacheTests
{
    [Fact]
    public void GetResourceDescriptorsPerAssembly_Locates_Identifiable_Resource()
    {
        // Arrange
        Type resourceClrType = typeof(Model);

        var assemblyCache = new ResourceDescriptorAssemblyCache();
        assemblyCache.RegisterAssembly(resourceClrType.Assembly);

        // Act
        IReadOnlyCollection<ResourceDescriptor> descriptors = assemblyCache.GetResourceDescriptors();

        // Assert
        descriptors.ShouldNotBeEmpty();
        descriptors.Should().ContainSingle(descriptor => descriptor.ResourceClrType == resourceClrType);
    }

    [Fact]
    public void GetResourceDescriptorsPerAssembly_Only_Contains_IIdentifiable_Types()
    {
        // Arrange
        Type resourceClrType = typeof(Model);

        var assemblyCache = new ResourceDescriptorAssemblyCache();
        assemblyCache.RegisterAssembly(resourceClrType.Assembly);

        // Act
        IReadOnlyCollection<ResourceDescriptor> descriptors = assemblyCache.GetResourceDescriptors();

        // Assert
        descriptors.ShouldNotBeEmpty();
        descriptors.Select(descriptor => descriptor.ResourceClrType).Should().AllBeAssignableTo<IIdentifiable>();
    }
}
