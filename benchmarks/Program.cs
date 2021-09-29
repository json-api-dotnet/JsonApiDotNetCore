using BenchmarkDotNet.Running;
using Benchmarks.Deserialization;
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
                typeof(ResourceDeserializationBenchmarks),
                typeof(OperationsDeserializationBenchmarks),
                typeof(JsonApiSerializerBenchmarks),
                typeof(QueryParserBenchmarks),
                typeof(LinkBuilderGetNamespaceFromPathBenchmarks)
            });

            switcher.Run(args);
        }
    }
}
