#nullable disable

using BenchmarkDotNet.Running;
using Benchmarks.Deserialization;
using Benchmarks.LinkBuilding;
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
                typeof(ResourceSerializationBenchmarks),
                typeof(OperationsSerializationBenchmarks),
                typeof(QueryParserBenchmarks),
                typeof(LinkBuilderGetNamespaceFromPathBenchmarks)
            });

            switcher.Run(args);
        }
    }
}
