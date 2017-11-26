using BenchmarkDotNet.Running;
using Benchmarks.Query;
using Benchmarks.Serialization;

namespace Benchmarks {
    class Program {
        static void Main(string[] args) {
            BenchmarkRunner.Run<JsonApiDeserializer_Benchmarks>();
            BenchmarkRunner.Run<JsonApiSerializer_Benchmarks>();
            BenchmarkRunner.Run<QueryParser_Benchmarks>();
        }
    }
}
