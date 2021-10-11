#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SubResource : Identifiable<int>
    {
        [Attr]
        public string Value { get; set; }
    }
}
