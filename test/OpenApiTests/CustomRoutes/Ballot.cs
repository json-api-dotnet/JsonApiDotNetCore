using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CustomRoutes")]
public sealed class Ballot : Identifiable<Guid>
{
    [Attr]
    public string VoterSocialSecurityNumber { get; set; } = null!;

    [HasOne]
    public Candidate? SelectedCandidate { get; set; }

    [HasOne]
    public Election Election { get; set; } = null!;
}
