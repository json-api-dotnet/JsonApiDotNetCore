using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class Article : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        public int? AuthorId { get; set; }

        [HasOne]
        public Person Author { get; set; }
        
        [HasMany]
        public List<Person> Reviewers { get; set; }
    }
}
