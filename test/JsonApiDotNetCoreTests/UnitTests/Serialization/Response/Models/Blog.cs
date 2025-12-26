using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Blog : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [HasOne]
    public required Person Reviewer { get; set; }

    [HasOne]
    public required Person Author { get; set; }
}
