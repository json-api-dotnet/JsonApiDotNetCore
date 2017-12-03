using BenchmarkDotNet.Running;
using Benchmarks.Query;
using Benchmarks.Serialization;

namespace Benchmarks {
    class Program {
        static void Main(string[] args) {
            var switcher = new BenchmarkSwitcher(new[] {
                typeof(JsonApiDeserializer_Benchmarks),
                typeof(JsonApiSerializer_Benchmarks),
                typeof(QueryParser_Benchmarks)
            });
            switcher.Run(args);
        }
    }
}
