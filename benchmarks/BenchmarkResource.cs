using JsonApiDotNetCore.Models;

namespace Benchmarks
{
    public class BenchmarkResource : Identifiable
    {
        [Attr(BenchmarkResourcePublicNames.NameAttr)] 
        public string Name { get; set; }

        [HasOne]
        public SubResource Child { get; set; }
    }

    public class SubResource : Identifiable
    {
        [Attr]
        public string Value { get; set; }
    }

    public static class BenchmarkResourcePublicNames
    {
        public const string NameAttr = "full-name";
        public const string Type = "simple-types";
    }
}
