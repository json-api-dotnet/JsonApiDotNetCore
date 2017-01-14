using System;

namespace JsonApiDotNetCore.Internal
{
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string publicName)
        {
            PublicAttributeName = publicName;
        }

        public string PublicAttributeName { get; set; }
        public string InternalAttributeName { get; set; }
    }
}
