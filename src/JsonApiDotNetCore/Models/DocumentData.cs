using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    public class DocumentData
    {
       public string Type { get; set; }
       public string Id { get; set; }
       public Dictionary<string,object> Attributes { get; set; }
    }
}
