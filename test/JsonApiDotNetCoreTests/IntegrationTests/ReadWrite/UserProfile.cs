using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ReadWrite")]
public sealed class UserProfile : Identifiable<long>
{
    [Attr]
    [Required]
    public bool? UseDarkMode { get; set; }

    [HasOne]
    public UserAccount? User { get; set; }
}
