using BenchmarkDotNet.Running;
using Benchmarks.JsonApiContext;
using Benchmarks.LinkBuilder;
using Benchmarks.Query;
using Benchmarks.RequestMiddleware;
using Benchmarks.Serialization;

namespace Benchmarks {
    class Program {
        static void Main(string[] args) {
            var switcher = new BenchmarkSwitcher(new[] {
                typeof(JsonApiDeserializer_Benchmarks),
                typeof(JsonApiSerializer_Benchmarks),
                typeof(QueryParser_Benchmarks),
                typeof(LinkBuilder_GetNamespaceFromPath_Benchmarks),
                typeof(ContainsMediaTypeParameters_Benchmarks),
                typeof(PathIsRelationship_Benchmarks)
            });
            switcher.Run(args);
        }
    }
}
