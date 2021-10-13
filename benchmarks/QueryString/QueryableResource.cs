using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Benchmarks.QueryString
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class QueryableResource : Identifiable<int>
    {
        [Attr(PublicName = "alt-attr-name")]
        public string? Name { get; set; }

        [HasOne]
        public QueryableResource? Child { get; set; }
    }
}
