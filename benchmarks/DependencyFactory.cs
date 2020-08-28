using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services.Contract;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Benchmarks
{
    internal static class DependencyFactory
    {
        public static IResourceGraph CreateResourceGraph(IJsonApiOptions options)
        {
            ResourceGraphBuilder builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.AddResource<BenchmarkResource>(BenchmarkResourcePublicNames.Type);
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
