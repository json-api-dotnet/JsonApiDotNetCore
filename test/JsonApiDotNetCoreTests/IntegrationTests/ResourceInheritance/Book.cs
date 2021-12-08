using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Book : ContentItem
{
    [Attr]
    public int PageCount { get; set; }
}
