using System;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Models
{
    public class LinksAttribute : Attribute
    {
        public LinksAttribute(LinkTypes links)
        {
            Links = links;
        }

        public LinkTypes Links { get; set; }
    }
}
