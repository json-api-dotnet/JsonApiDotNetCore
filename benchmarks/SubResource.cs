using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks
{
    public sealed class SubResource : Identifiable
    {
        [Attr]
        public string Value { get; set; }
    }
}
