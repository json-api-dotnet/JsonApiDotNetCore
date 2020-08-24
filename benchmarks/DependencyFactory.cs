using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Benchmarks
{
    internal static class DependencyFactory
    {
        public static IResourceGraph CreateResourceGraph(IJsonApiOptions options)
        {
            IResourceGraphBuilder builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.Add<BenchmarkResource>(BenchmarkResourcePublicNames.Type);
            return builder.Build();
        }

        public static IResourceDefinitionProvider CreateResourceDefinitionProvider(IResourceGraph resourceGraph)
        {
            var resourceDefinition = new ResourceDefinition<BenchmarkResource>(resourceGraph);

            var resourceDefinitionProviderMock = new Mock<IResourceDefinitionProvider>();
            resourceDefinitionProviderMock.Setup(provider => provider.Get(It.IsAny<Type>())).Returns(resourceDefinition);
            
            return resourceDefinitionProviderMock.Object;
        }
    }
}
