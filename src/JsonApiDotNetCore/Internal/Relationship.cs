using System;

namespace JsonApiDotNetCore.Internal
{
    public class Relationship
    {
        public Type Type { get; set; }
        public string RelationshipName { get; set; }
    }
}
