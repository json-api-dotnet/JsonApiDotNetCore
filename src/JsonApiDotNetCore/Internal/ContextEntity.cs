using System;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal
{
    public class ContextEntity
    {
        public string EntityName { get; set; }
        public Type EntityType { get; set; }
        public List<AttrAttribute> Attributes { get; set; }
        public List<Relationship> Relationships { get; set; }
    }
}
