using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class Human : Identifiable<int>
{
    [Attr]
    public string FamilyName { get; set; } = null!;

    [Attr]
    public bool IsRetired { get; set; }

    [HasOne]
    public HealthInsurance? HealthInsurance { get; set; }

    [HasMany]
    public ICollection<Human> Parents { get; set; } = new List<Human>();

    [HasMany]
    public ICollection<ContentItem> FavoriteContent { get; set; } = new List<ContentItem>();
}
