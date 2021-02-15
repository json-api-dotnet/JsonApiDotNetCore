using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks
{
    public sealed class BenchmarkResource : Identifiable
    {
        [Attr(PublicName = BenchmarkResourcePublicNames.NameAttr)] 
        public string Name { get; set; }

        [HasOne]
        public SubResource Child { get; set; }
    }
}
