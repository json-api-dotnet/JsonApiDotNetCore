using BenchmarkDotNet.Running;
using Benchmarks.Deserialization;
using Benchmarks.QueryString;
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
                typeof(QueryStringParserBenchmarks)
            });

            switcher.Run(args);
        }
    }
}
