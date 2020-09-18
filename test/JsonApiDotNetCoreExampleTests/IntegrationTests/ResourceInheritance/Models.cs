using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public abstract class Person : Identifiable { }
    
    public sealed class Male : Person
    {
        [Attr]
        public string MaleProperty { get; set; }
    }
    
    public sealed class Female : Person
    {
        [Attr]
        public string FemaleProperty { get; set; }
    }
    
    public sealed class Placeholder : Identifiable
    {
        [HasOne]
        public Person OneToOnePerson { get; set; }
        
        public int? OneToOnePersonId { get; set; }
        
        [HasMany]
        public List<Person> OneToManyPersons { get; set; }
        
        [NotMapped]
        [HasManyThrough(nameof(PlaceholderPersons))]
        public List<Person> ManyToManyPersons { get; set; }
        
        public List<PlaceholderPerson> PlaceholderPersons { get; set; }
    }
    
    public sealed class PlaceholderPerson
    {
        public int PlaceHolderId { get; set; }
        
        public Placeholder PlaceHolder { get; set; }

        public int PersonId { get; set; }
        
        public Person Person { get; set; }
    }
}
