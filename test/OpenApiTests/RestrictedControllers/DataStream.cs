using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.RestrictedControllers", GenerateControllerEndpoints = JsonApiEndpoints.None)]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public sealed class DataStream : Identifiable<long>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    [Attr]
    [Required]
    public ulong? BytesTransmitted { get; set; }
}
