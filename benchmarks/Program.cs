using BenchmarkDotNet.Running;
using Benchmarks.LinkBuilder;
using Benchmarks.Query;
using Benchmarks.Serialization;

namespace Benchmarks
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(JsonApiDeserializerBenchmarks),
                typeof(JsonApiSerializerBenchmarks),
                typeof(QueryParserBenchmarks),
                typeof(LinkBuilderGetNamespaceFromPathBenchmarks)
            });

            switcher.Run(args);
        }
    }
}
