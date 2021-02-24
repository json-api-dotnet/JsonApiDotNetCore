using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Tag : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
