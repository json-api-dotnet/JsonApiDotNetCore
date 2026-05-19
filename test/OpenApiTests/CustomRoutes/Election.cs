using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.CustomRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.CustomRoutes")]
public sealed class Election : Identifiable<Guid>
{
    [Attr]
    [Required]
    public DateOnly? Date { get; set; }

    [HasMany(PublicName = "contenders")]
    public ISet<Candidate> Candidates { get; set; } = new HashSet<Candidate>();

    [HasMany(PublicName = "votes")]
    public ISet<Ballot> Ballots { get; set; } = new HashSet<Ballot>();
}
