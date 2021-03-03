using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class Human : Identifiable
    {
        [Attr]
        public string FamilyName { get; set; }

        [Attr]
        public bool IsRetired { get; set; }

        [HasOne]
        public HealthInsurance HealthInsurance { get; set; }

        [HasMany]
        public ICollection<Human> Parents { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(HumanFavoriteContentItems))]
        public ICollection<ContentItem> FavoriteContent { get; set; }

        public ICollection<HumanFavoriteContentItem> HumanFavoriteContentItems { get; set; }
    }
}
