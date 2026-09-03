using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation")]
public sealed class LoginToken : Identifiable<long>
{
    [Attr]
    public string Value { get; set; } = null!;

    [Attr]
    [Required]
    public DateTimeOffset? CreatedAt { get; set; }
}
