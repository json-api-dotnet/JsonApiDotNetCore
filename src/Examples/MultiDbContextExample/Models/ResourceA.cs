using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MultiDbContextExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceA : Identifiable<int>
    {
        [Attr]
        public string? NameA { get; set; }
    }
}
