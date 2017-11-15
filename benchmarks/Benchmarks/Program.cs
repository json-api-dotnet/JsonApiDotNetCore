using BenchmarkDotNet.Running;
using Benchmarks.Serialization;

namespace Benchmarks {
    class Program {
        static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<JsonApiDeserializer_Benchmarks>();
        }
    }
}
