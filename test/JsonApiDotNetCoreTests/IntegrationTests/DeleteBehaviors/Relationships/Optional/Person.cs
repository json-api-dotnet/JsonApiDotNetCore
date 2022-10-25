using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.DeleteBehaviors.Relationships.Optional;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.DeleteBehaviors.Relationships.Optional")]
public sealed class Person : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public IList<Post> Posts { get; } = new List<Post>();

    [HasOne]
    public Blog? OwnedBlog { get; set; }
}
