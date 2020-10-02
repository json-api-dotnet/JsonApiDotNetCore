using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models
{
    public abstract class Human : Identifiable
    {
        [Attr]
        public bool Retired { get; set; }
        
        [HasOne]
        public Animal Pet { get; set; }
        
        [HasMany]
        public List<Human> Parents { get; set; }
        
        [NotMapped]
        [HasManyThrough(nameof(HumanContentItems))]
        public List<Content> FavoriteContent { get; set; }
        
        public List<HumanContentItem> HumanContentItems { get; set; }
    }
}
    

    

