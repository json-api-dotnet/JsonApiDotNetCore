using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.DeleteBehaviors.Relationships.Optional;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.DeleteBehaviors.Relationships.Optional")]
public sealed class Post : Identifiable<int>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public string Content { get; set; } = null!;

    [HasOne]
    public Blog? Blog { get; set; }

    [HasOne]
    public Person? Author { get; set; }
}
