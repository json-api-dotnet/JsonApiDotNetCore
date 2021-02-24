using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SubResource : Identifiable
    {
        [Attr]
        public string Value { get; set; }
    }
}
