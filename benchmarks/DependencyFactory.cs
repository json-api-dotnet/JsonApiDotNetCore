using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks
{
    internal sealed class DependencyFactory
    {
        public IResourceGraph CreateResourceGraph(IJsonApiOptions options)
        {
            var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
            builder.Add<BenchmarkResource>(BenchmarkResourcePublicNames.Type);
            return builder.Build();
        }
    }
}
