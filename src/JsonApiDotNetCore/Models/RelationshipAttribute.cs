using System;

namespace JsonApiDotNetCore.Models
{
    public class RelationshipAttribute : Attribute
    {
        protected RelationshipAttribute(string publicName)
        {
            PublicRelationshipName = publicName;
        }

        public string PublicRelationshipName { get; set; }
        public string InternalRelationshipName { get; set; }
    }
}
