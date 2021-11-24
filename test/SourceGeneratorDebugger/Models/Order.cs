using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace SourceGeneratorDebugger.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.All)]
    public sealed class Order : Identifiable<long>
    {
        [Attr]
        public decimal TotalAmount { get; set; }
    }
}
