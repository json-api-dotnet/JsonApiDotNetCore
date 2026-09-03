using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods")]
public sealed class CupOfCoffee : Identifiable<long>
{
    [Attr]
    [Required]
    public bool? HasSugar { get; set; }

    [Attr]
    [Required]
    public bool? HasMilk { get; set; }
}
