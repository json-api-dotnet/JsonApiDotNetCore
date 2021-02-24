using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MultiDbContextExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceB : Identifiable
    {
        [Attr]
        public string NameB { get; set; }
    }
}
