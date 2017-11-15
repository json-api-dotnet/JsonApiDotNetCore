using BenchmarkDotNet.Running;
using Benchmarks.Serialization;

namespace Benchmarks {
    class Program {
        static void Main(string[] args) {
            BenchmarkRunner.Run<JsonApiDeserializer_Benchmarks>();
            BenchmarkRunner.Run<JsonApiSerializer_Benchmarks>();
        }
    }
}
