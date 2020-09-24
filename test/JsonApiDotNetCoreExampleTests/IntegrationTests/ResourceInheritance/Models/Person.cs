using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public abstract class Person : Identifiable
    {
        [HasOne]
        public Animal Pet { get; set; }

        [HasMany]
        public List<Person> Parents { get; set; }
        
        [NotMapped]
        [HasManyThrough(nameof(PersonLiterature))]
        public List<Literature> FavoriteLiterature { get; set; }
        
        public List<LiteraturePerson> PersonLiterature { get; set; }
    }
}
    

    

