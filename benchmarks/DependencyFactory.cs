using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks
{
    internal static class DependencyFactory
    {
        public static IResourceGraph CreateResourceGraph(IJsonApiOptions options)
        {
            ResourceGraphBuilder builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.Add<BenchmarkResource>(BenchmarkResourcePublicNames.Type);
            return builder.Build();
        }
    }
}
