using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class Placeholder : Identifiable
    {
        [HasOne]
        public Person OneToOnePerson { get; set; }

        [HasMany]
        public List<Person> OneToManyPersons { get; set; }
        
        [NotMapped]
        [HasManyThrough(nameof(PlaceholderPersons))]
        public List<Person> ManyToManyPersons { get; set; }
        
        public List<PlaceholderPerson> PlaceholderPersons { get; set; }
    }
}
