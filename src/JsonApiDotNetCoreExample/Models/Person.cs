using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCoreExample.Models
{
    public class Person : Identifiable<int>, IHasMeta
    {
        public override int Id { get; set; }
        
        [Attr("first-name")]
        public string FirstName { get; set; }

        [Attr("last-name")]
        public string LastName { get; set; }

        public virtual List<TodoItem> TodoItems { get; set; }

        public Dictionary<string, object> GetMeta(IJsonApiContext context)
        {
            return new Dictionary<string, object> {
                { "copyright", "Copyright 2015 Example Corp." },
                { "authors", new string[] { "Jared Nance" } }
            };
        }
    }
}
