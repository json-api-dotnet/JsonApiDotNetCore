using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Headers")]
public sealed class Country : Identifiable<Guid>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public long Population { get; set; }

    [HasMany]
    public ISet<Language> Languages { get; set; } = new HashSet<Language>();
}
