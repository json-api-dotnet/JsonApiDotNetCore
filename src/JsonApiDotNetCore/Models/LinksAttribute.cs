using System;

namespace JsonApiDotNetCore.Models
{
    public class LinksAttribute : Attribute
    {
        public LinksAttribute(Link links)
        {
            Links = links;
        }

        public Link Links { get; set; }
    }
}
